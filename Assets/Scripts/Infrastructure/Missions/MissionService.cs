using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Infrastructure.Wallet;

namespace HackingProject.Infrastructure.Missions
{
    public sealed class MissionService : IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly List<MissionDefinitionSO> _completedMissions = new List<MissionDefinitionSO>();
        private readonly HashSet<string> _completedMissionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rewardedMissionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private MissionDefinitionSO _activeMission;
        private bool[] _objectiveStates = Array.Empty<bool>();
        private bool _missionCompleted;
        private MissionCatalogSO _catalog;
        private readonly WalletService _walletService;

        public MissionService(EventBus eventBus, WalletService walletService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _walletService = walletService;
            _subscriptions.Add(_eventBus.Subscribe<TerminalCommandExecutedEvent>(OnTerminalCommandExecuted));
            _subscriptions.Add(_eventBus.Subscribe<FileManagerOpenedFileEvent>(OnFileManagerOpenedFile));
        }

        public MissionDefinitionSO ActiveMission => _activeMission;
        public bool IsActiveMissionCompleted => _missionCompleted;
        public IReadOnlyList<MissionDefinitionSO> CompletedMissions => _completedMissions;

        public bool IsObjectiveCompleted(int index)
        {
            return _objectiveStates != null && index >= 0 && index < _objectiveStates.Length && _objectiveStates[index];
        }

        public void SetActiveMission(MissionDefinitionSO mission)
        {
            _activeMission = mission;
            var count = mission?.Objectives?.Count ?? 0;
            _objectiveStates = count > 0 ? new bool[count] : Array.Empty<bool>();
            _missionCompleted = false;

            if (mission != null)
            {
                _eventBus.Publish(new MissionStartedEvent(mission));
                TryCompleteMission();
            }
        }

        public void SetCatalog(MissionCatalogSO catalog)
        {
            _catalog = catalog;
        }

        public void Dispose()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i]?.Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnTerminalCommandExecuted(TerminalCommandExecutedEvent evt)
        {
            if (_activeMission == null)
            {
                return;
            }

            var objectives = _activeMission.Objectives;
            if (objectives == null || objectives.Count == 0)
            {
                return;
            }

            for (var i = 0; i < objectives.Count; i++)
            {
                if (IsObjectiveCompleted(i))
                {
                    continue;
                }

                var objective = objectives[i];
                if (objective == null || objective.Type != MissionObjectiveType.TerminalCommand)
                {
                    continue;
                }

                if (!CommandMatches(objective.Command, evt.Command))
                {
                    continue;
                }

                if (!PathMatches(objective.Path, evt.ResolvedPath))
                {
                    continue;
                }

                CompleteObjective(i);
            }
        }

        private void OnFileManagerOpenedFile(FileManagerOpenedFileEvent evt)
        {
            if (_activeMission == null)
            {
                return;
            }

            var objectives = _activeMission.Objectives;
            if (objectives == null || objectives.Count == 0)
            {
                return;
            }

            for (var i = 0; i < objectives.Count; i++)
            {
                if (IsObjectiveCompleted(i))
                {
                    continue;
                }

                var objective = objectives[i];
                if (objective == null || objective.Type != MissionObjectiveType.FileOpened)
                {
                    continue;
                }

                if (!PathMatches(objective.Path, evt.FullPath))
                {
                    continue;
                }

                CompleteObjective(i);
            }
        }

        private void CompleteObjective(int index)
        {
            if (index < 0 || index >= _objectiveStates.Length)
            {
                return;
            }

            if (_objectiveStates[index])
            {
                return;
            }

            _objectiveStates[index] = true;
            _eventBus.Publish(new MissionObjectiveCompletedEvent(_activeMission, index));

            TryCompleteMission();
        }

        private void TryCompleteMission()
        {
            if (_missionCompleted || _activeMission == null)
            {
                return;
            }

            if (!AreAllObjectivesComplete())
            {
                return;
            }

            _missionCompleted = true;
            TrackCompletion(_activeMission);
            _eventBus.Publish(new MissionCompletedEvent(_activeMission));
            TryGrantReward(_activeMission);
            TryStartNextMission();
        }

        private bool AreAllObjectivesComplete()
        {
            for (var i = 0; i < _objectiveStates.Length; i++)
            {
                if (!_objectiveStates[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void TrackCompletion(MissionDefinitionSO mission)
        {
            if (mission == null)
            {
                return;
            }

            var missionId = GetMissionId(mission);
            if (string.IsNullOrWhiteSpace(missionId))
            {
                return;
            }

            if (_completedMissionIds.Add(missionId))
            {
                _completedMissions.Add(mission);
            }
        }

        private void TryGrantReward(MissionDefinitionSO mission)
        {
            if (mission == null || _walletService == null)
            {
                return;
            }

            var reward = mission.RewardCredits;
            if (reward == 0)
            {
                return;
            }

            var missionId = GetMissionId(mission);
            if (string.IsNullOrWhiteSpace(missionId) || !_rewardedMissionIds.Add(missionId))
            {
                return;
            }

            _walletService.AddCredits(reward, $"Mission {missionId}");
            _eventBus.Publish(new MissionRewardGrantedEvent(missionId, reward));
        }

        private void TryStartNextMission()
        {
            if (_catalog == null || _activeMission == null || _catalog.Missions == null)
            {
                return;
            }

            var missions = _catalog.Missions;
            var index = missions.IndexOf(_activeMission);
            if (index < 0)
            {
                return;
            }

            for (var i = index + 1; i < missions.Count; i++)
            {
                var next = missions[i];
                if (next == null)
                {
                    continue;
                }

                var nextId = GetMissionId(next);
                if (!string.IsNullOrWhiteSpace(nextId) && _completedMissionIds.Contains(nextId))
                {
                    continue;
                }

                SetActiveMission(next);
                break;
            }
        }

        private static string GetMissionId(MissionDefinitionSO mission)
        {
            if (mission == null)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(mission.Id) ? mission.name : mission.Id;
        }

        private static bool CommandMatches(string expected, string actual)
        {
            if (string.IsNullOrWhiteSpace(expected))
            {
                return true;
            }

            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PathMatches(string expected, string actual)
        {
            if (string.IsNullOrWhiteSpace(expected))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(actual))
            {
                return false;
            }

            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }
    }
}
