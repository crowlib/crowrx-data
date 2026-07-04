using UnityEngine;

namespace CrowRx.Data
{
    internal static class BrokerInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad() => Broker.Release();
    }
}