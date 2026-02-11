using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Notifications;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Infrastructure.Wallet;

namespace HackingProject.Systems.Store
{
    public sealed class StoreService
    {
        private const string DownloadsPath = "/home/user/downloads";
        private const string InstallerExtension = ".installer";
        private const int InstallerVersion = 1;

        private readonly EventBus _eventBus;
        private readonly WalletService _walletService;
        private readonly SaveGameData _saveData;
        private readonly NotificationService _notificationService;
        private readonly VirtualFileSystem _vfs;

        public StoreService(EventBus eventBus, WalletService walletService, SaveGameData saveData, NotificationService notificationService, VirtualFileSystem vfs)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _notificationService = notificationService;
            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            EnsureOwnedList();
        }

        public bool CanPurchase(StoreItemDefinitionSO item)
        {
            if (item == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.AppIdToInstall))
            {
                return false;
            }

            if (IsOwned(item.AppIdToInstall))
            {
                return false;
            }

            return _walletService.Credits >= Math.Max(0, item.PriceCredits);
        }

        public bool IsOwned(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            var owned = _saveData.OwnedAppIds;
            if (owned == null)
            {
                return false;
            }

            return owned.Contains(appId);
        }

        public bool Purchase(StoreItemDefinitionSO item)
        {
            if (item == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.AppIdToInstall))
            {
                return false;
            }

            EnsureOwnedList();
            if (IsOwned(item.AppIdToInstall))
            {
                return false;
            }

            var downloads = _vfs.Resolve(DownloadsPath) as VfsDirectory;
            if (downloads == null)
            {
                return false;
            }

            var displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
            var price = Math.Max(0, item.PriceCredits);
            if (!_walletService.TrySpendCredits(price, $"Purchase {displayName}"))
            {
                return false;
            }

            var installerName = GetInstallerFileName(downloads, item.AppIdToInstall);
            var package = InstallerPackage.Create(item.AppIdToInstall, displayName, InstallerVersion, price);
            downloads.AddFile(installerName, package.ToJson());

            _saveData.OwnedAppIds.Add(item.AppIdToInstall);
            _eventBus.Publish(new StorePurchaseCompletedEvent(item.ItemId, item.AppIdToInstall, price));
            _notificationService?.Post($"Downloaded installer: {installerName}");
            return true;
        }

        private static string GetInstallerFileName(VfsDirectory downloads, string appId)
        {
            var baseName = $"{appId}{InstallerExtension}";
            if (downloads.GetChild(baseName) == null)
            {
                return baseName;
            }

            var index = 1;
            while (true)
            {
                var candidate = $"{appId}_{index}{InstallerExtension}";
                if (downloads.GetChild(candidate) == null)
                {
                    return candidate;
                }

                index++;
            }
        }

        private void EnsureOwnedList()
        {
            if (_saveData.OwnedAppIds == null)
            {
                _saveData.OwnedAppIds = new List<string>();
            }
        }
    }
}
