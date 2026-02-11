using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Missions;
using HackingProject.Infrastructure.Save;
using HackingProject.Systems.Store;
using UnityEngine;

namespace HackingProject.Systems.Save
{
    public interface ISaveWriter
    {
        void Save(SaveGameData data);
    }

    public sealed class SaveServiceWriter : ISaveWriter
    {
        private readonly SaveService _service;

        public SaveServiceWriter(SaveService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Save(SaveGameData data)
        {
            _service.Save(data);
        }
    }

    public sealed class AutoSaveService : IDisposable
    {
        private const float DefaultDebounceSeconds = 1.5f;
        private const string MissionCompletedReason = "MissionCompleted";
        private const string MissionRewardReason = "MissionReward";
        private const string StorePurchaseReason = "StorePurchase";
        private const string AppInstalledReason = "AppInstalled";

        private readonly EventBus _eventBus;
        private readonly ISaveWriter _saveWriter;
        private readonly float _debounceSeconds;
        private SaveGameData _saveData;
        private float _timeRemaining;
        private bool _pending;
        private string _pendingReason;

        private readonly IDisposable _missionCompletedSubscription;
        private readonly IDisposable _missionRewardSubscription;
        private readonly IDisposable _storePurchaseSubscription;
        private readonly IDisposable _appInstalledSubscription;

        public AutoSaveService(EventBus eventBus, ISaveWriter saveWriter, SaveGameData saveData, float debounceSeconds = DefaultDebounceSeconds)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _saveWriter = saveWriter ?? throw new ArgumentNullException(nameof(saveWriter));
            _saveData = saveData;
            _debounceSeconds = debounceSeconds > 0f ? debounceSeconds : DefaultDebounceSeconds;

            _missionCompletedSubscription = _eventBus.Subscribe<MissionCompletedEvent>(_ => ScheduleSave(MissionCompletedReason));
            _missionRewardSubscription = _eventBus.Subscribe<MissionRewardGrantedEvent>(_ => ScheduleSave(MissionRewardReason));
            _storePurchaseSubscription = _eventBus.Subscribe<StorePurchaseCompletedEvent>(_ => ScheduleSave(StorePurchaseReason));
            _appInstalledSubscription = _eventBus.Subscribe<AppInstalledEvent>(_ => ScheduleSave(AppInstalledReason));
        }

        public void SetSaveData(SaveGameData data)
        {
            _saveData = data;
        }

        public void Tick(float deltaTime)
        {
            if (!_pending)
            {
                return;
            }

            _timeRemaining -= deltaTime;
            if (_timeRemaining > 0f)
            {
                return;
            }

            PerformSave();
        }

        public void Dispose()
        {
            _missionCompletedSubscription?.Dispose();
            _missionRewardSubscription?.Dispose();
            _storePurchaseSubscription?.Dispose();
            _appInstalledSubscription?.Dispose();
        }

        private void ScheduleSave(string reason)
        {
            if (_saveData == null)
            {
                return;
            }

            _pending = true;
            _timeRemaining = _debounceSeconds;
            _pendingReason = reason;
        }

        private void PerformSave()
        {
            if (_saveData == null)
            {
                _pending = false;
                return;
            }

            _pending = false;
            _eventBus.Publish(new SaveSessionCaptureEvent(_saveData.OsSession));
            _saveWriter.Save(_saveData);
            _eventBus.Publish(new SaveCompletedEvent(_saveData));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrWhiteSpace(_pendingReason))
            {
                Debug.Log($"[AutoSave] {_pendingReason}");
            }
#endif

            _pendingReason = null;
        }
    }
}
