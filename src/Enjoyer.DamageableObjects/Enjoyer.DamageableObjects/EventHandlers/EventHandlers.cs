using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.Configs;
using Enjoyer.DamageableObjects.Patches.Scp096;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Scp096;
using System.Collections.Generic;
using System.Linq;
using MapEvents = Exiled.Events.Handlers.Map;
using Scp096Events = Exiled.Events.Handlers.Scp096;

namespace Enjoyer.DamageableObjects;

internal class EventHandlers
{
    internal virtual void RegisterEvents()
    {
        MapEvents.Generated += OnGenerated;
        Scp096Events.Enraging += OnEnraging;
    }

    internal virtual void UnregisterEvents()
    {
        MapEvents.Generated -= OnGenerated;
        Scp096Events.Enraging -= OnEnraging;
    }

    private static void OnGenerated()
    {
        foreach (KeyValuePair<DoorType, DamageableDoorsProperties> pair in DoPlugin.PluginConfig.DamageableDoorTypes)
        {
            foreach (BreakableDoor door in Door.List.Where(door => door.Type == pair.Key).Select(door => door.As<BreakableDoor>()))
            {
                DamageableDoor component = door.GameObject.AddComponent<DamageableDoor>();
                component.Door = door;
                component.MaxHealth = pair.Value.MaxHealth;
                component.ProtectionEfficacy = pair.Value.DamageResistance;
                component.AllowedDamageTypes = pair.Value.AllowedDamageTypes;
                component.NotAffectToDamage = pair.Value.NotAffectToDamage;
                component.HitMarkerSize = DoPlugin.PluginConfig.DoorHitMarkerSize;
            }
        }
    }

    private static void OnEnraging(EnragingEventArgs ev) => ChargingProcessHitsPatch._processedComponents.Remove(ev.Player);
}
