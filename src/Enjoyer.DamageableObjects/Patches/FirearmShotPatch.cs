using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using System;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyObstacleDamage))]
internal static class FirearmShotPatch
{
    private static void Finalizer(Exception? __exception)
    {
        if (__exception != null) Logger.Error(__exception);
    }

    private static void Postfix(HitscanHitregModuleBase __instance, HitRayPair hitInfo)
    {
        RaycastHit hit = hitInfo.Hit;
        if (hit.transform.GetComponentInParent<DamageableComponent>() is not { } damageable) return;

        damageable.OnShot(new ShotArgs(__instance, hit.distance));
    }
}