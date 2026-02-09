using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Wallet
{
    public readonly struct CreditsChangedEvent : IEvent
    {
        public CreditsChangedEvent(int previousCredits, int currentCredits, int delta, string reason)
        {
            PreviousCredits = previousCredits;
            CurrentCredits = currentCredits;
            Delta = delta;
            Reason = reason;
        }

        public int PreviousCredits { get; }
        public int CurrentCredits { get; }
        public int Delta { get; }
        public string Reason { get; }
    }
}
