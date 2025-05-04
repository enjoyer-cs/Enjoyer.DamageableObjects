using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.Configs;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using System.Collections.Generic;
using System.Linq;
using MapEvents = Exiled.Events.Handlers.Map;

namespace Enjoyer.DamageableObjects;

internal class EventHandlers
{
    internal virtual void RegisterEvents() => MapEvents.Generated += OnGenerated;

    internal virtual void UnregisterEvents() => MapEvents.Generated += OnGenerated;

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
}
