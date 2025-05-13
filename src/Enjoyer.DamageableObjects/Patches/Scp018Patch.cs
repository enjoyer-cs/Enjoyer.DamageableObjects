using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Footprinting;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(Scp018Projectile), nameof(Scp018Projectile.RegisterBounce))]
internal static class Scp018Patch
{
    private static Dictionary<Scp018Projectile, List<DamageableComponent>> _processedComponents { get; } = [];

    private static void HandleDetection(Scp018Projectile scp018, Collider collider, Footprint? previousOwner)
    {
        try
        {
            List<DamageableComponent> ignoreComponents = _processedComponents.GetOrAdd(scp018, () => []);
            if (!collider.transform.TryGetComponentInParent(out DamageableComponent damageable) || ignoreComponents.Contains(damageable))
                return;

            damageable.OnScp018Bounce(scp018, previousOwner?.Hub);
            ignoreComponents.Add(damageable);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static void Postfix(Scp018Projectile __instance) => _processedComponents.Remove(__instance);

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

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

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
