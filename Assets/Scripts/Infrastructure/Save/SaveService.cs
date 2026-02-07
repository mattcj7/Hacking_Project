using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HackingProject.Infrastructure.Save
{
    public sealed class SaveService
    {
        public const string PrimaryFileName = "savegame.json";
        public const string BackupFileName = "savegame.bak.json";
        public const string TempFileName = "savegame.tmp.json";

        private readonly string _baseDirectory;
        private readonly string _primaryPath;
        private readonly string _backupPath;
        private readonly string _tempPath;

        public SaveService(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new ArgumentException("Base directory is required.", nameof(baseDirectory));
            }

            _baseDirectory = baseDirectory;
            _primaryPath = Path.Combine(_baseDirectory, PrimaryFileName);
            _backupPath = Path.Combine(_baseDirectory, BackupFileName);
            _tempPath = Path.Combine(_baseDirectory, TempFileName);
        }

        public SaveLoadSource LastLoadSource { get; private set; }

        public bool TryLoad(out SaveGameData data)
        {
            data = null;
            LastLoadSource = SaveLoadSource.None;

            if (!File.Exists(_primaryPath))
            {
                return false;
            }

            if (TryLoadFromPath(_primaryPath, out data))
            {
                LastLoadSource = SaveLoadSource.Primary;
                return true;
            }

            if (File.Exists(_backupPath) && TryLoadFromPath(_backupPath, out data))
            {
                LastLoadSource = SaveLoadSource.Backup;
                return true;
            }

            data = null;
            LastLoadSource = SaveLoadSource.None;
            return false;
        }

        public void Save(SaveGameData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.Version = data.Version <= 0 ? 1 : data.Version;
            data.LastSavedUtcIso = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

            var payloadJson = JsonUtility.ToJson(data);
            var envelope = new SaveEnvelope
            {
                Version = 1,
                PayloadJson = payloadJson,
                PayloadSha256Hex = ComputeSha256Hex(payloadJson)
            };

            var envelopeJson = JsonUtility.ToJson(envelope);

            Directory.CreateDirectory(_baseDirectory);
            File.WriteAllText(_tempPath, envelopeJson, Encoding.UTF8);

            ReplacePrimaryWithTemp();
        }

        public void DeleteAll()
        {
            DeleteFile(_primaryPath);
            DeleteFile(_backupPath);
        }

        private bool TryLoadFromPath(string path, out SaveGameData data)
        {
            data = null;

            try
            {
                var envelopeJson = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(envelopeJson))
                {
                    return false;
                }

                var envelope = JsonUtility.FromJson<SaveEnvelope>(envelopeJson);
                if (envelope == null || string.IsNullOrEmpty(envelope.PayloadJson) || string.IsNullOrEmpty(envelope.PayloadSha256Hex))
                {
                    return false;
                }

                var computedHash = ComputeSha256Hex(envelope.PayloadJson);
                if (!string.Equals(computedHash, envelope.PayloadSha256Hex, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                data = JsonUtility.FromJson<SaveGameData>(envelope.PayloadJson);
                return data != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReplacePrimaryWithTemp()
        {
            if (File.Exists(_primaryPath))
            {
                try
                {
                    File.Replace(_tempPath, _primaryPath, _backupPath, true);
                    return;
                }
                catch (PlatformNotSupportedException)
                {
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }

            if (File.Exists(_primaryPath))
            {
                File.Copy(_primaryPath, _backupPath, true);
                File.Delete(_primaryPath);
            }

            if (File.Exists(_tempPath))
            {
                File.Move(_tempPath, _primaryPath);
            }
        }

        private static string ComputeSha256Hex(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);
            var builder = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
