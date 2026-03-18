namespace Ledger;

public static class ServiceLocator
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Initialize(IServiceProvider services)
    {
        Services = services;
    }
}