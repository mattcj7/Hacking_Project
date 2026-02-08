using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Terminal;
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
            var service = new MissionService(eventBus);
            var mission = CreateMission();

            service.SetActiveMission(mission);

            eventBus.Publish(new TerminalCommandExecutedEvent("cd", new[] { "docs" }, "/home/user", "/home/user/docs"));

            Assert.IsTrue(service.IsObjectiveCompleted(0));
            Assert.IsFalse(service.IsObjectiveCompleted(1));
        }

        [Test]
        public void Mission_Completes_WhenAllObjectivesSatisfied()
        {
            var eventBus = new EventBus();
            var service = new MissionService(eventBus);
            var mission = CreateMission();
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

        private static MissionDefinitionSO CreateMission()
        {
            var mission = ScriptableObject.CreateInstance<MissionDefinitionSO>();
            mission.Id = "M001";
            mission.Title = "Read The Docs";
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
