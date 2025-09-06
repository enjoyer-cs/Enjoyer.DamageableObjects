using Enjoyer.DamageableObjects.Patches.Scp096;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Handlers;
using PlayerRoles.PlayableScps.Scp096;

namespace Enjoyer.DamageableObjects.EventHandlers;

internal class EventHandlers
{
    internal virtual void RegisterEvents()
    {
        Scp096Events.Charging += OnCharging;
    }

    internal virtual void UnregisterEvents()
    {
        Scp096Events.Charging -= OnCharging;
    }

    private static void OnCharging(Scp096ChargingEventArgs ev) => ProcessHitsPatch._chargeAttackedComponents.Remove((Scp096Role)ev.Player.RoleBase);
}
