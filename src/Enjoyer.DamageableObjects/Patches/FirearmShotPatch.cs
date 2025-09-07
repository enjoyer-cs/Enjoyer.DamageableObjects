using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using UnityEngine;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyObstacleDamage))]
internal static class FirearmShotPatch
{
    private static void Postfix(HitscanHitregModuleBase __instance, HitRayPair hitInfo)
    {
        RaycastHit hit = hitInfo.Hit;
        if (hit.transform.GetComponentInParent<DamageableComponent>() is not { } damageable) return;

        damageable.OnShot(new ShotArgs(__instance, hit.distance));
    }
}
