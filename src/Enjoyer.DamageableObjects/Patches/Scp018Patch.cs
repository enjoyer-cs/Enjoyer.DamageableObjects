using Enjoyer.DamageableObjects.API.Components;
using Footprinting;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Pool;
using static HarmonyLib.AccessTools;
using Logger = LabApi.Features.Console.Logger;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(Scp018Projectile), nameof(Scp018Projectile.RegisterBounce))]
internal static class Scp018Patch
{
    private static Dictionary<Scp018Projectile, List<DamageableComponent>> _processedComponents { get; } = [];

    private static void HandleDetection(Scp018Projectile scp018, Collider collider, Footprint? previousOwner)
    {
        try
        {
            List<DamageableComponent> ignoreComponents = _processedComponents.GetOrAddNew(scp018);

            if (!collider.transform.TryGetComponentInParent(out DamageableComponent damageable) || ignoreComponents.Contains(damageable))
                return;

            damageable.OnScp018Bounce(scp018, previousOwner?.Hub);
            ignoreComponents.Add(damageable);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private static void Finalizer(Exception? __exception)
    {
        if (__exception != null) Logger.Error(__exception);
    }

    private static void Postfix(Scp018Projectile __instance) => _processedComponents.Remove(__instance);

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        ListPool<CodeInstruction>.Get(out List<CodeInstruction> newInstructions);
        newInstructions.AddRange(instructions);

        // Index, after save raycastHit to local with index 1 in foreach
        int targetIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Stloc_2) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(ItemPickupBase), nameof(ItemPickupBase.PreviousOwner))),
                new(OpCodes.Call, Method(typeof(Scp018Patch), nameof(HandleDetection)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Release(newInstructions);
    }
}
