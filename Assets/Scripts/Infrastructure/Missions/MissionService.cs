using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;

namespace HackingProject.Infrastructure.Missions
{
    public sealed class MissionService : IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private MissionDefinitionSO _activeMission;
        private bool[] _objectiveStates = Array.Empty<bool>();
        private bool _missionCompleted;

        public MissionService(EventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _subscriptions.Add(_eventBus.Subscribe<TerminalCommandExecutedEvent>(OnTerminalCommandExecuted));
            _subscriptions.Add(_eventBus.Subscribe<FileManagerOpenedFileEvent>(OnFileManagerOpenedFile));
        }

        public MissionDefinitionSO ActiveMission => _activeMission;
        public bool IsActiveMissionCompleted => _missionCompleted;

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
            _eventBus.Publish(new MissionCompletedEvent(_activeMission));
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
