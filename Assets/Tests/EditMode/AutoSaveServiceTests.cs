using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Save;
using HackingProject.Systems.Save;
using HackingProject.Systems.Store;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class AutoSaveServiceTests
    {
        [Test]
        public void Debounce_CoalescesMultipleEventsIntoSingleSave()
        {
            var eventBus = new EventBus();
            var saveData = SaveGameData.CreateDefault();
            var writer = new FakeSaveWriter();
            var autoSave = new AutoSaveService(eventBus, writer, saveData, 1.5f);

            eventBus.Publish(new MissionCompletedEvent(null));
            eventBus.Publish(new MissionRewardGrantedEvent("M001", 10));

            autoSave.Tick(1.4f);
            Assert.AreEqual(0, writer.SaveCount);

            autoSave.Tick(0.2f);
            Assert.AreEqual(1, writer.SaveCount);
        }

        [Test]
        public void AutoSave_Triggers_OnKeyEvents()
        {
            var eventBus = new EventBus();
            var saveData = SaveGameData.CreateDefault();
            var writer = new FakeSaveWriter();
            var autoSave = new AutoSaveService(eventBus, writer, saveData, 0.1f);

            eventBus.Publish(new StorePurchaseCompletedEvent("item", "Notes", 25));
            autoSave.Tick(0.11f);
            Assert.AreEqual(1, writer.SaveCount);

            eventBus.Publish(new AppInstalledEvent("Notes"));
            autoSave.Tick(0.11f);
            Assert.AreEqual(2, writer.SaveCount);

            eventBus.Publish(new MissionCompletedEvent(null));
            autoSave.Tick(0.11f);
            Assert.AreEqual(3, writer.SaveCount);

            eventBus.Publish(new MissionRewardGrantedEvent("M001", 5));
            autoSave.Tick(0.11f);
            Assert.AreEqual(4, writer.SaveCount);
        }

        private sealed class FakeSaveWriter : ISaveWriter
        {
            public int SaveCount { get; private set; }

            public void Save(SaveGameData data)
            {
                SaveCount++;
            }
        }
    }
}
