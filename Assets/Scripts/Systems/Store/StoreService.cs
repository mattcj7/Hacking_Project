using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Notifications;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Wallet;

namespace HackingProject.Systems.Store
{
    public sealed class StoreService
    {
        private readonly EventBus _eventBus;
        private readonly WalletService _walletService;
        private readonly SaveGameData _saveData;
        private readonly NotificationService _notificationService;

        public StoreService(EventBus eventBus, WalletService walletService, SaveGameData saveData, NotificationService notificationService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _notificationService = notificationService;
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

            var price = Math.Max(0, item.PriceCredits);
            if (!_walletService.TrySpendCredits(price, $"Purchase {item.DisplayName}"))
            {
                return false;
            }

            _saveData.OwnedAppIds.Add(item.AppIdToInstall);
            _eventBus.Publish(new StorePurchaseCompletedEvent(item.ItemId, item.AppIdToInstall, price));
            var displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
            _notificationService?.Post($"Purchased {displayName}");
            return true;
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
