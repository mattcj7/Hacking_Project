using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Notifications;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Wallet;
using HackingProject.Systems.Store;
using NUnit.Framework;
using UnityEngine;

namespace HackingProject.Tests.EditMode
{
    public sealed class StoreServiceTests
    {
        [Test]
        public void Purchase_Fails_WhenInsufficientCredits()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 0);
            var saveData = SaveGameData.CreateDefault();
            var storeService = new StoreService(eventBus, wallet, saveData, new NotificationService(eventBus));
            var item = CreateItem("Notes", 10);

            var result = storeService.Purchase(item);

            Assert.IsFalse(result);
            Assert.AreEqual(0, wallet.Credits);
            Assert.IsFalse(saveData.OwnedAppIds.Contains("Notes"));
        }

        [Test]
        public void Purchase_DeductsCreditsAndMarksOwned()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 50);
            var saveData = SaveGameData.CreateDefault();
            var storeService = new StoreService(eventBus, wallet, saveData, new NotificationService(eventBus));
            var item = CreateItem("Notes", 25);

            var result = storeService.Purchase(item);

            Assert.IsTrue(result);
            Assert.AreEqual(25, wallet.Credits);
            Assert.IsTrue(saveData.OwnedAppIds.Contains("Notes"));
        }

        [Test]
        public void Install_AddsInstalledAndIsIdempotent()
        {
            var eventBus = new EventBus();
            var saveData = SaveGameData.CreateDefault();
            saveData.OwnedAppIds.Add("Notes");
            var installService = new InstallService(eventBus, saveData);

            var installedCount = 0;
            eventBus.Subscribe<AppInstalledEvent>(_ => installedCount++);

            var first = installService.Install("Notes");
            var second = installService.Install("Notes");

            Assert.IsTrue(first);
            Assert.IsFalse(second);
            Assert.AreEqual(1, installedCount);
            Assert.AreEqual(1, saveData.InstalledAppIds.Count);
            Assert.IsTrue(saveData.InstalledAppIds.Contains("Notes"));
        }

        private static StoreItemDefinitionSO CreateItem(string appId, int price)
        {
            var item = ScriptableObject.CreateInstance<StoreItemDefinitionSO>();
            item.ItemId = appId;
            item.DisplayName = appId;
            item.PriceCredits = price;
            item.AppIdToInstall = appId;
            return item;
        }
    }
}
