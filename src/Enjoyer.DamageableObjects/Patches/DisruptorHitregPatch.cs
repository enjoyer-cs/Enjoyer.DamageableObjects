﻿using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches;

/// <summary>
///     Патч метода <see cref="DisruptorHitregModule.PrescanSingle" />,
///     Нужен для обработки урона одиночного выстрела Particle Disruptor
/// </summary>
[HarmonyPatch(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.PrescanSingle))]
internal static class DisruptorHitregPatch
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

        LocalBuilder ownerLocal = generator.DeclareLocal(typeof(ReferenceHub));

        newInstructions.InsertRange(0, new List<CodeInstruction>
        {
            // ReferenceHub owner = this.Owner
            new(OpCodes.Ldarg_0),
            new(OpCodes.Callvirt, PropertyGetter(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.Owner))),
            new(OpCodes.Stloc_S, ownerLocal.LocalIndex)
        });

        // Index, after save raycastHit to local with index 1 in foreach
        int targetIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Stloc_1) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldloc_S, ownerLocal.LocalIndex),
                new(OpCodes.Call, Method(typeof(DisruptorHitregPatch), nameof(HandleHit)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
