using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Wallet;
using NUnit.Framework;
using UnityEngine;

namespace HackingProject.Tests.EditMode
{
    public sealed class MissionServiceTests
    {
        [Test]
        public void TerminalCommandEvent_CompletesExpectedObjective()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 0);
            var service = new MissionService(eventBus, wallet);
            var mission = CreateMission("M001", 0);

            service.SetActiveMission(mission);

            eventBus.Publish(new TerminalCommandExecutedEvent("cd", new[] { "docs" }, "/home/user", "/home/user/docs"));

            Assert.IsTrue(service.IsObjectiveCompleted(0));
            Assert.IsFalse(service.IsObjectiveCompleted(1));
        }

        [Test]
        public void Mission_Completes_WhenAllObjectivesSatisfied()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 0);
            var service = new MissionService(eventBus, wallet);
            var mission = CreateMission("M001", 0);
            var completedCount = 0;

            eventBus.Subscribe<MissionCompletedEvent>(_ => completedCount++);
            service.SetActiveMission(mission);

            eventBus.Publish(new TerminalCommandExecutedEvent("cd", new[] { "docs" }, "/home/user", "/home/user/docs"));
            eventBus.Publish(new TerminalCommandExecutedEvent("cat", new[] { "readme.txt" }, "/home/user/docs", "/home/user/docs/readme.txt"));
            eventBus.Publish(new TerminalCommandExecutedEvent("cat", new[] { "readme.txt" }, "/home/user/docs", "/home/user/docs/readme.txt"));

            Assert.IsTrue(service.IsObjectiveCompleted(0));
            Assert.IsTrue(service.IsObjectiveCompleted(1));
            Assert.IsTrue(service.IsActiveMissionCompleted);
            Assert.AreEqual(1, completedCount);
        }

        [Test]
        public void MissionCompletion_AwardsCreditsOnce_AndFiresCreditsChanged()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 0);
            var service = new MissionService(eventBus, wallet);
            var mission = CreateMission("M001", 25);
            var creditsChanged = 0;
            var rewardGranted = 0;
            var lastDelta = 0;

            eventBus.Subscribe<CreditsChangedEvent>(evt =>
            {
                creditsChanged++;
                lastDelta = evt.Delta;
            });
            eventBus.Subscribe<MissionRewardGrantedEvent>(_ => rewardGranted++);

            service.SetActiveMission(mission);

            eventBus.Publish(new TerminalCommandExecutedEvent("cd", new[] { "docs" }, "/home/user", "/home/user/docs"));
            eventBus.Publish(new TerminalCommandExecutedEvent("cat", new[] { "readme.txt" }, "/home/user/docs", "/home/user/docs/readme.txt"));
            eventBus.Publish(new TerminalCommandExecutedEvent("cat", new[] { "readme.txt" }, "/home/user/docs", "/home/user/docs/readme.txt"));

            Assert.AreEqual(25, wallet.Credits);
            Assert.AreEqual(1, creditsChanged);
            Assert.AreEqual(25, lastDelta);
            Assert.AreEqual(1, rewardGranted);
        }

        [Test]
        public void MissionChain_StartsNextMissionInCatalog_WhenRequested()
        {
            var eventBus = new EventBus();
            var wallet = new WalletService(eventBus, 0);
            var service = new MissionService(eventBus, wallet);
            var catalog = ScriptableObject.CreateInstance<MissionCatalogSO>();
            var first = CreateMission("M001", 0);
            var second = CreateMission("M002", 0);
            catalog.Missions = new List<MissionDefinitionSO> { first, second };

            service.SetCatalog(catalog);
            service.SetActiveMission(first);

            eventBus.Publish(new TerminalCommandExecutedEvent("cd", new[] { "docs" }, "/home/user", "/home/user/docs"));
            eventBus.Publish(new TerminalCommandExecutedEvent("cat", new[] { "readme.txt" }, "/home/user/docs", "/home/user/docs/readme.txt"));

            Assert.IsTrue(service.IsActiveMissionCompleted);
            Assert.IsTrue(service.TryGetNextMission(out var next));
            Assert.AreSame(second, next);
            Assert.IsTrue(service.TryStartNextMission());
            Assert.AreSame(second, service.ActiveMission);
        }

        private static MissionDefinitionSO CreateMission(string id, int rewardCredits)
        {
            var mission = ScriptableObject.CreateInstance<MissionDefinitionSO>();
            mission.Id = id;
            mission.Title = "Read The Docs";
            mission.RewardCredits = rewardCredits;
            mission.Objectives = new List<MissionObjectiveDefinition>
            {
                new MissionObjectiveDefinition
                {
                    Type = MissionObjectiveType.TerminalCommand,
                    Description = "In Terminal, cd docs",
                    Command = "cd",
                    Path = "/home/user/docs"
                },
                new MissionObjectiveDefinition
                {
                    Type = MissionObjectiveType.TerminalCommand,
                    Description = "In Terminal, cat readme.txt",
                    Command = "cat",
                    Path = "/home/user/docs/readme.txt"
                }
            };
            return mission;
        }
    }
}
