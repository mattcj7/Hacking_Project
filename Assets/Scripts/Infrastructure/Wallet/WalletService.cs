using System;
using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Wallet
{
    public sealed class WalletService
    {
        private readonly EventBus _eventBus;
        private int _credits;

        public WalletService(EventBus eventBus, int startingCredits)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _credits = startingCredits;
        }

        public int Credits => _credits;

        public void AddCredits(int amount, string reason)
        {
            if (amount == 0)
            {
                return;
            }

            var previous = _credits;
            _credits += amount;
            _eventBus.Publish(new CreditsChangedEvent(previous, _credits, amount, reason));
        }
    }
}
