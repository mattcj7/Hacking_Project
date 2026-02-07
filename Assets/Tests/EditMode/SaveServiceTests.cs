using System;
using System.IO;
using System.Text;
using HackingProject.Infrastructure.Save;
using NUnit.Framework;
using UnityEngine;

namespace HackingProject.Tests.EditMode
{
    public sealed class SaveServiceTests
    {
        private string _tempDirectory;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "HackingProject.SaveTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Test]
        public void TryLoad_MissingFile_ReturnsFalse()
        {
            var service = new SaveService(_tempDirectory);

            var result = service.TryLoad(out var data);

            Assert.IsFalse(result);
            Assert.IsNull(data);
            Assert.AreEqual(SaveLoadSource.None, service.LastLoadSource);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var service = new SaveService(_tempDirectory);
            var data = SaveGameData.CreateDefault();
            data.Credits = 42;

            service.Save(data);

            var result = service.TryLoad(out var loaded);

            Assert.IsTrue(result);
            Assert.AreEqual(SaveLoadSource.Primary, service.LastLoadSource);
            Assert.AreEqual(data.Version, loaded.Version);
            Assert.AreEqual(42, loaded.Credits);
            Assert.IsFalse(string.IsNullOrEmpty(loaded.LastSavedUtcIso));
        }

        [Test]
        public void TryLoad_UsesBackupWhenPrimaryCorrupt()
        {
            var service = new SaveService(_tempDirectory);
            var data = SaveGameData.CreateDefault();
            data.Credits = 7;
            service.Save(data);

            var primaryPath = Path.Combine(_tempDirectory, SaveService.PrimaryFileName);
            var backupPath = Path.Combine(_tempDirectory, SaveService.BackupFileName);

            File.Copy(primaryPath, backupPath, true);
            File.WriteAllText(primaryPath, "not json", Encoding.UTF8);

            var result = service.TryLoad(out var loaded);

            Assert.IsTrue(result);
            Assert.AreEqual(SaveLoadSource.Backup, service.LastLoadSource);
            Assert.AreEqual(7, loaded.Credits);
        }

        [Test]
        public void TryLoad_RejectsHashMismatch()
        {
            var service = new SaveService(_tempDirectory);
            var primaryPath = Path.Combine(_tempDirectory, SaveService.PrimaryFileName);

            var payload = JsonUtility.ToJson(new SaveGameData
            {
                Version = 1,
                Credits = 3,
                LastSavedUtcIso = "2026-02-07T00:00:00Z"
            });

            var envelope = new SaveEnvelope
            {
                Version = 1,
                PayloadJson = payload,
                PayloadSha256Hex = "deadbeef"
            };

            var envelopeJson = JsonUtility.ToJson(envelope);
            File.WriteAllText(primaryPath, envelopeJson, Encoding.UTF8);

            var result = service.TryLoad(out var loaded);

            Assert.IsFalse(result);
            Assert.IsNull(loaded);
        }
    }
}
