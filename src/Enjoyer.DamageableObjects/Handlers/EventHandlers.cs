using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.API.Extensions;
using Enjoyer.DamageableObjects.Configs;
using Enjoyer.DamageableObjects.Patches.Scp096;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Scp1509;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp096;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Enjoyer.DamageableObjects.Handlers;

internal class EventHandlers
{
    internal virtual void RegisterEvents()
    {
        ServerEvents.MapGenerated += OnGenerated;
        Scp096Events.Charging += OnCharging;

        PlayerEvents.ProcessingJailbirdMessage += OnProcessingJailbirdMessage;
        PlayerEvents.ProcessingScp1509Message += OnProcessingScp1509Message;
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

    private static void OnProcessingJailbirdMessage(PlayerProcessingJailbirdMessageEventArgs ev)
    {
        if (ev.Message != JailbirdMessageType.AttackPerformed || RaycastDamageable(ev.Player) is not { } damageable) return;

        damageable.OnJailbirdHit(ev.Player.ReferenceHub, ev.JailbirdItem.Base);
    }

    private static void OnProcessingScp1509Message(PlayerProcessingScp1509MessageEventArgs ev)
    {
        if (ev.Message != Scp1509MessageType.AttackPerformed || RaycastDamageable(ev.Player) is not { } damageable) return;

        damageable.OnScp1509Hit(ev.Player.ReferenceHub, ev.Scp1509Item.Base);
    }

    private static DamageableComponent? RaycastDamageable(Player player)
    {
        if (Physics.Raycast(player.Camera.position, player.Camera.forward, out RaycastHit hit, 2,
                new CachedLayerMask("Default", "Door", "Glass")) &&
            hit.transform.GetComponentInParent<DamageableComponent>() is { } damageable)
            return damageable;

        return null;
    }
}