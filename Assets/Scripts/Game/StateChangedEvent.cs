using HackingProject.Infrastructure.Events;

namespace HackingProject.Game
{
    public readonly struct StateChangedEvent : IEvent
    {
        public StateChangedEvent(string previousState, string nextState)
        {
            PreviousState = previousState;
            NextState = nextState;
        }

        public string PreviousState { get; }
        public string NextState { get; }
    }
}
