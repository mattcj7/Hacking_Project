using HackingProject.Infrastructure.Events;

namespace HackingProject.Systems.Store
{
    public readonly struct StorePurchaseCompletedEvent : IEvent
    {
        public StorePurchaseCompletedEvent(string itemId, string appId, int priceCredits)
        {
            ItemId = itemId;
            AppId = appId;
            PriceCredits = priceCredits;
        }

        public string ItemId { get; }
        public string AppId { get; }
        public int PriceCredits { get; }
    }
}
