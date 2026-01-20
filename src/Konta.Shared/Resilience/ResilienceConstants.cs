namespace Konta.Shared.Resilience;

public static class ResilienceConstants
{
    public const string DefaultPolicy = "DefaultResilience";
    
    public static class Retry
    {
        public const int Count = 3;
        public static readonly TimeSpan Delay = TimeSpan.FromSeconds(2);
    }

    public static class CircuitBreaker
    {
        public const double FailureThreshold = 0.5; // 50% d'échec
        public static readonly TimeSpan SamplingDuration = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(15);
    }
}
