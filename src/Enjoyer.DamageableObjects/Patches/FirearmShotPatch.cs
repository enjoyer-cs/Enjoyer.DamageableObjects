using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using UnityEngine;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyDestructibleDamage))]
internal static class FirearmShotPatch
{
    private static void Prefix(HitscanHitregModuleBase __instance, DestructibleHitPair target)
    {
        RaycastHit hit = target.Raycast.Hit;
        if (hit.transform.GetComponentInParent<DamageableComponent>() is not { } damageable) return;

        damageable.OnShot(new ShotArgs(__instance, hit.distance));
    }
}
