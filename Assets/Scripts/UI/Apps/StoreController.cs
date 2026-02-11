using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Wallet;
using HackingProject.Systems.Store;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class StoreController : IDisposable
    {
        private const string ListName = "store-list";
        private const string ItemClassName = "store-item";
        private const string HeaderClassName = "store-item-header";
        private const string NameClassName = "store-item-name";
        private const string StateClassName = "store-item-state";
        private const string DescriptionClassName = "store-item-description";
        private const string PriceClassName = "store-item-price";
        private const string ActionClassName = "store-item-action";

        private readonly VisualElement _root;
        private readonly StoreCatalogSO _catalog;
        private readonly StoreService _storeService;
        private readonly InstallService _installService;
        private readonly EventBus _eventBus;
        private readonly ScrollView _list;
        private IDisposable _purchaseSubscription;
        private IDisposable _installSubscription;
        private IDisposable _creditsSubscription;
        private bool _subscriptionsWired;

        public StoreController(VisualElement root, StoreCatalogSO catalog, StoreService storeService, InstallService installService, EventBus eventBus)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _catalog = catalog;
            _storeService = storeService;
            _installService = installService;
            _eventBus = eventBus;
            _list = root.Q<ScrollView>(ListName);
        }

        public void Initialize()
        {
            Refresh();
            if (_eventBus != null && !_subscriptionsWired)
            {
                _purchaseSubscription = _eventBus.Subscribe<StorePurchaseCompletedEvent>(_ => Refresh());
                _installSubscription = _eventBus.Subscribe<AppInstalledEvent>(_ => Refresh());
                _creditsSubscription = _eventBus.Subscribe<CreditsChangedEvent>(_ => Refresh());
                _root.RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
                _subscriptionsWired = true;
            }
        }

        public void Dispose()
        {
            _purchaseSubscription?.Dispose();
            _installSubscription?.Dispose();
            _creditsSubscription?.Dispose();
        }

        private void Refresh()
        {
            if (_list == null)
            {
                return;
            }

            _list.Clear();
            if (_catalog == null || _catalog.Items == null || _catalog.Items.Count == 0)
            {
                _list.Add(new Label("No items available."));
                return;
            }

            for (var i = 0; i < _catalog.Items.Count; i++)
            {
                var item = _catalog.Items[i];
                if (item == null)
                {
                    continue;
                }

                _list.Add(BuildItemRow(item));
            }
        }

        private VisualElement BuildItemRow(StoreItemDefinitionSO item)
        {
            var row = new VisualElement();
            row.AddToClassList(ItemClassName);

            var header = new VisualElement();
            header.AddToClassList(HeaderClassName);

            var name = new Label(string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName);
            name.AddToClassList(NameClassName);

            var state = new Label(BuildStateText(item));
            state.AddToClassList(StateClassName);

            header.Add(name);
            header.Add(state);

            var description = new Label(item.Description ?? string.Empty);
            description.AddToClassList(DescriptionClassName);

            var price = new Label($"Price: {item.PriceCredits} credits");
            price.AddToClassList(PriceClassName);

            var action = new Button(() => OnActionClicked(item))
            {
                text = BuildActionText(item)
            };
            action.AddToClassList(ActionClassName);
            action.SetEnabled(CanPerformAction(item));

            row.Add(header);
            row.Add(description);
            row.Add(price);
            row.Add(action);
            return row;
        }

        private string BuildStateText(StoreItemDefinitionSO item)
        {
            if (_installService != null && _installService.IsInstalled(item.AppIdToInstall))
            {
                return "Installed";
            }

            if (_storeService != null && _storeService.IsOwned(item.AppIdToInstall))
            {
                return "Owned";
            }

            return "Available";
        }

        private string BuildActionText(StoreItemDefinitionSO item)
        {
            if (_installService != null && _installService.IsInstalled(item.AppIdToInstall))
            {
                return "Installed";
            }

            if (_storeService != null && _storeService.IsOwned(item.AppIdToInstall))
            {
                return "Install";
            }

            return "Buy";
        }

        private bool CanPerformAction(StoreItemDefinitionSO item)
        {
            if (_storeService == null || _installService == null)
            {
                return false;
            }

            if (_installService.IsInstalled(item.AppIdToInstall))
            {
                return false;
            }

            if (_storeService.IsOwned(item.AppIdToInstall))
            {
                return true;
            }

            return _storeService.CanPurchase(item);
        }

        private void OnActionClicked(StoreItemDefinitionSO item)
        {
            if (_storeService == null || _installService == null)
            {
                return;
            }

            if (_installService.IsInstalled(item.AppIdToInstall))
            {
                return;
            }

            if (_storeService.IsOwned(item.AppIdToInstall))
            {
                _installService.Install(item.AppIdToInstall);
                Refresh();
                return;
            }

            _storeService.Purchase(item);
            Refresh();
        }
    }
}
