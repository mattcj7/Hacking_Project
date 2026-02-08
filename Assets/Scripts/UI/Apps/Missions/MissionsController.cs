using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using UnityEngine.UIElements;

namespace HackingProject.UI.Apps
{
    public sealed class MissionsController : IDisposable
    {
        private const string TitleName = "mission-title";
        private const string DescriptionName = "mission-description";
        private const string ObjectivesName = "objectives-root";
        private const string ObjectiveClassName = "mission-objective";
        private const string ObjectiveCompleteClassName = "mission-objective-complete";

        private readonly VisualElement _root;
        private readonly MissionService _missionService;
        private readonly EventBus _eventBus;
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private readonly VisualElement _objectivesRoot;
        private IDisposable _startedSubscription;
        private IDisposable _objectiveSubscription;
        private IDisposable _completedSubscription;
        private bool _subscriptionsWired;

        public MissionsController(VisualElement root, MissionService missionService, EventBus eventBus)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _missionService = missionService;
            _eventBus = eventBus;
            _titleLabel = root.Q<Label>(TitleName);
            _descriptionLabel = root.Q<Label>(DescriptionName);
            _objectivesRoot = root.Q<VisualElement>(ObjectivesName);
        }

        public void Initialize()
        {
            Refresh();
            if (_eventBus != null && !_subscriptionsWired)
            {
                _startedSubscription = _eventBus.Subscribe<MissionStartedEvent>(_ => Refresh());
                _objectiveSubscription = _eventBus.Subscribe<MissionObjectiveCompletedEvent>(_ => Refresh());
                _completedSubscription = _eventBus.Subscribe<MissionCompletedEvent>(_ => Refresh());
                _root.RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
                _subscriptionsWired = true;
            }
        }

        public void Dispose()
        {
            _startedSubscription?.Dispose();
            _objectiveSubscription?.Dispose();
            _completedSubscription?.Dispose();
        }

        private void Refresh()
        {
            if (_missionService == null || _missionService.ActiveMission == null)
            {
                if (_titleLabel != null)
                {
                    _titleLabel.text = "No Active Mission";
                }

                if (_descriptionLabel != null)
                {
                    _descriptionLabel.text = string.Empty;
                }

                _objectivesRoot?.Clear();
                return;
            }

            var mission = _missionService.ActiveMission;
            if (_titleLabel != null)
            {
                _titleLabel.text = string.IsNullOrWhiteSpace(mission.Title) ? mission.name : mission.Title;
            }

            if (_descriptionLabel != null)
            {
                _descriptionLabel.text = mission.Description ?? string.Empty;
            }

            if (_objectivesRoot == null)
            {
                return;
            }

            _objectivesRoot.Clear();
            var objectives = mission.Objectives;
            if (objectives == null || objectives.Count == 0)
            {
                _objectivesRoot.Add(new Label("No objectives."));
                return;
            }

            for (var i = 0; i < objectives.Count; i++)
            {
                var objective = objectives[i];
                var isComplete = _missionService.IsObjectiveCompleted(i);
                var label = new Label($"{(isComplete ? "[x]" : "[ ]")} {BuildObjectiveText(objective)}");
                label.AddToClassList(ObjectiveClassName);
                if (isComplete)
                {
                    label.AddToClassList(ObjectiveCompleteClassName);
                }

                label.style.marginBottom = i < objectives.Count - 1 ? 4f : 0f;
                _objectivesRoot.Add(label);
            }
        }

        private static string BuildObjectiveText(MissionObjectiveDefinition objective)
        {
            if (objective == null)
            {
                return "Objective";
            }

            if (!string.IsNullOrWhiteSpace(objective.Description))
            {
                return objective.Description;
            }

            if (objective.Type == MissionObjectiveType.TerminalCommand)
            {
                if (!string.IsNullOrWhiteSpace(objective.Path))
                {
                    return $"{objective.Command} {objective.Path}";
                }

                return objective.Command;
            }

            if (!string.IsNullOrWhiteSpace(objective.Path))
            {
                return $"Open {objective.Path}";
            }

            return "Objective";
        }
    }
}
