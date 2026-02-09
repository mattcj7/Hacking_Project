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
        private const string RewardName = "mission-reward";
        private const string CompleteName = "mission-complete";
        private const string ObjectivesName = "missions-objectives";
        private const string CompletedName = "missions-completed";
        private const string NextMissionButtonName = "next-mission-button";
        private const string NoMoreLabelName = "no-more-label";
        private const string ObjectiveClassName = "mission-objective";
        private const string ObjectiveCompleteClassName = "mission-objective-complete";
        private const string CompletedEntryClassName = "mission-completed-entry";

        private readonly VisualElement _root;
        private readonly MissionService _missionService;
        private readonly EventBus _eventBus;
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private readonly Label _rewardLabel;
        private readonly Label _completeLabel;
        private readonly VisualElement _objectivesRoot;
        private readonly VisualElement _completedRoot;
        private readonly Button _nextMissionButton;
        private readonly Label _noMoreLabel;
        private MissionDefinitionSO _cachedNextMission;
        private IDisposable _startedSubscription;
        private IDisposable _objectiveSubscription;
        private IDisposable _completedSubscription;
        private bool _subscriptionsWired;
        private bool _nextButtonWired;

        public MissionsController(VisualElement root, MissionService missionService, EventBus eventBus)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _missionService = missionService;
            _eventBus = eventBus;
            _titleLabel = root.Q<Label>(TitleName);
            _descriptionLabel = root.Q<Label>(DescriptionName);
            _rewardLabel = root.Q<Label>(RewardName);
            _completeLabel = root.Q<Label>(CompleteName);
            _objectivesRoot = root.Q<VisualElement>(ObjectivesName);
            _completedRoot = root.Q<VisualElement>(CompletedName);
            _nextMissionButton = root.Q<Button>(NextMissionButtonName);
            _noMoreLabel = root.Q<Label>(NoMoreLabelName);
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

            if (!_nextButtonWired && _nextMissionButton != null)
            {
                _nextMissionButton.clicked += OnNextMissionClicked;
                _nextButtonWired = true;
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

                UpdateRewardLabel(null);
                SetCompletedVisible(false);
                _objectivesRoot?.Clear();
                UpdateCompletedList();
                UpdateNextMissionControls();
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

            UpdateRewardLabel(mission);
            SetCompletedVisible(_missionService.IsActiveMissionCompleted);
            if (_objectivesRoot == null)
            {
                UpdateCompletedList();
                UpdateNextMissionControls();
                return;
            }

            _objectivesRoot.Clear();
            var objectives = mission.Objectives;
            if (objectives == null || objectives.Count == 0)
            {
                _objectivesRoot.Add(new Label("No objectives."));
                UpdateCompletedList();
                UpdateNextMissionControls();
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

            UpdateCompletedList();
            UpdateNextMissionControls();
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

        private void SetCompletedVisible(bool isVisible)
        {
            if (_completeLabel == null)
            {
                return;
            }

            _completeLabel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateRewardLabel(MissionDefinitionSO mission)
        {
            if (_rewardLabel == null)
            {
                return;
            }

            if (mission == null || mission.RewardCredits == 0)
            {
                _rewardLabel.text = string.Empty;
                _rewardLabel.style.display = DisplayStyle.None;
                return;
            }

            var prefix = _missionService != null && _missionService.IsActiveMissionCompleted ? "Reward Granted" : "Reward";
            _rewardLabel.text = $"{prefix}: +{mission.RewardCredits} credits";
            _rewardLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateCompletedList()
        {
            if (_completedRoot == null || _missionService == null)
            {
                return;
            }

            _completedRoot.Clear();
            var completed = _missionService.CompletedMissions;
            if (completed == null || completed.Count == 0)
            {
                _completedRoot.Add(new Label("No completed missions."));
                return;
            }

            for (var i = 0; i < completed.Count; i++)
            {
                var mission = completed[i];
                if (mission == null)
                {
                    continue;
                }

                var title = string.IsNullOrWhiteSpace(mission.Title) ? mission.name : mission.Title;
                var rewardText = mission.RewardCredits > 0 ? $" (+{mission.RewardCredits} credits)" : string.Empty;
                var label = new Label($"{title} — Completed{rewardText}");
                label.AddToClassList(CompletedEntryClassName);
                label.style.marginBottom = i < completed.Count - 1 ? 4f : 0f;
                _completedRoot.Add(label);
            }
        }

        private void UpdateNextMissionControls()
        {
            if (_nextMissionButton == null && _noMoreLabel == null)
            {
                return;
            }

            MissionDefinitionSO nextMission = null;
            var hasNext = _missionService != null
                && _missionService.IsActiveMissionCompleted
                && _missionService.TryGetNextMission(out nextMission);
            _cachedNextMission = hasNext ? nextMission : null;

            if (_nextMissionButton != null)
            {
                _nextMissionButton.style.display = hasNext ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasNext && _cachedNextMission != null)
                {
                    var title = string.IsNullOrWhiteSpace(_cachedNextMission.Title) ? _cachedNextMission.name : _cachedNextMission.Title;
                    _nextMissionButton.text = $"Start {title}";
                }
            }

            if (_noMoreLabel != null)
            {
                var showNoMore = _missionService != null && _missionService.IsActiveMissionCompleted && !hasNext;
                _noMoreLabel.style.display = showNoMore ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnNextMissionClicked()
        {
            if (_missionService == null || _cachedNextMission == null)
            {
                return;
            }

            _missionService.SetActiveMission(_cachedNextMission);
            Refresh();
        }
    }
}
