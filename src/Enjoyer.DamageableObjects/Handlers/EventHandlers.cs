using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.API.Extensions;
using Enjoyer.DamageableObjects.Configs;
using Enjoyer.DamageableObjects.Patches.Scp096;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp096;
using System.Collections.Generic;
using System.Linq;

namespace Enjoyer.DamageableObjects.Handlers;

internal class EventHandlers
{
    internal virtual void RegisterEvents()
    {
        ServerEvents.MapGenerated += OnGenerated;
        Scp096Events.Charging += OnCharging;
    }

    internal virtual void UnregisterEvents()
    {
        ServerEvents.MapGenerated -= OnGenerated;
        Scp096Events.Charging -= OnCharging;
    }

    private static void OnGenerated(MapGeneratedEventArgs ev)
    {
        foreach (KeyValuePair<string, DamageableDoorsProperties> pair in DoPlugin.PluginConfig.DamageableDoors)
        {
            foreach (BreakableDoor door in BreakableDoor.List.Where(door => door.GetDoorNameOrZone() == pair.Key))
            {
                DoPlugin.SendDebug($"Adding {nameof(DamageableDoor)} to {door.Base.name}");

                DamageableDoor component = door.GameObject.AddDamageableComponent<DamageableDoor>(pair.Value);
                component.Door = door;
                component.NotAffectToDamage = pair.Value.NotAffectToDamage;
                component.HitMarkerSize = DoPlugin.PluginConfig.DoorHitMarkerSize;
            }
        }
    }

    private static void OnCharging(Scp096ChargingEventArgs ev) =>
        ProcessHitsPatch._chargeAttackedComponents.Remove((Scp096Role)ev.Player.RoleBase);
}