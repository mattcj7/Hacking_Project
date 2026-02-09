namespace HackingProject.Infrastructure.Wallet
{
    public interface IWalletServiceProvider
    {
        WalletService WalletService { get; }
    }
}
