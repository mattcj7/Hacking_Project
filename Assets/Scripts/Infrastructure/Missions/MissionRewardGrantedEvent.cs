using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Missions
{
    public readonly struct MissionRewardGrantedEvent : IEvent
    {
        public MissionRewardGrantedEvent(string missionId, int rewardCredits)
        {
            MissionId = missionId;
            RewardCredits = rewardCredits;
        }

        public string MissionId { get; }
        public int RewardCredits { get; }
    }
}
