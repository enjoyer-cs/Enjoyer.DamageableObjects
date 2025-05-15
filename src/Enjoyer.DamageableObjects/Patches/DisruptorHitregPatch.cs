using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.PrescanSingle))]
public static class DisruptorHitregPatch
{
    private static void HandleHit(DisruptorHitregModule module, RaycastHit? hit, ReferenceHub? player)
    {
        try
        {
            Log.Debug(hit.HasValue);
            if (!hit.HasValue || !hit.Value.transform.TryGetComponentInParent(out DamageableComponent damageable))
                return;

            Log.Debug(damageable);

            float damage = module.DamageAtDistance(hit.Value.distance) *
                           Mathf.Pow(1f / module._singleShotDivisionPerTarget, module._serverPenetrations);

            if (damageable.OnDisruptorSingleShot(player, damage))
                module._serverPenetrations++;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        // Index, after save raycastHit to local with index 1 in foreach
        int targetIndex = newInstructions.FindIndex(i => i.Is(OpCodes.Newobj, Constructor(typeof(HitRayPair)))) + 2;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, PropertyGetter(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.Owner))),
                new(OpCodes.Call, Method(typeof(DisruptorHitregPatch), nameof(HandleHit)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
