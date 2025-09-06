using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Subroutines;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Pool;
using static HarmonyLib.AccessTools;
using Logger = LabApi.Features.Console.Logger;

namespace Enjoyer.DamageableObjects.Patches.Scp939;

/// <summary>
///     Патч метода <see cref="Scp939Motor.OverlapCapsule" />, нужен для нанесения урона во время использования
///     <see cref="Scp939LungeAbility" />.
/// </summary>
[HarmonyPatch(typeof(Scp939Motor), nameof(Scp939Motor.OverlapCapsule))]
internal static class MotorOverlapCapsulePatch
{
    /// <summary>
    ///     Компоненты, обработанные при последнем использовании <see cref="Scp939LungeAbility" />.
    /// </summary>
    internal static Dictionary<Scp939LungeAbility, List<DamageableComponent>> _processedComponents { get; } = [];

    private static void HandleDetection(Collider detection, ReferenceHub player, Scp939LungeAbility lunge)
    {
        try
        {
            List<DamageableComponent>? ignoreComponents = _processedComponents.GetOrAdd(lunge, () => []);

            if (detection.GetComponentInParent<DamageableComponent>() is not { } damageable ||
                ignoreComponents.Contains(damageable) || !damageable.OnLunging(player, lunge, _processedComponents[lunge].IsEmpty()))
                return;

            ignoreComponents.Add(damageable);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        ListPool<CodeInstruction>.Get(out List<CodeInstruction> newInstructions);
        newInstructions.AddRange(instructions);

        LocalBuilder ownerLocal = generator.DeclareLocal(typeof(ReferenceHub));
        FieldInfo lungeField = Field(typeof(Scp939Motor), nameof(Scp939Motor._lunge));

        // ReferenceHub owner = this._lunge.Owner
        newInstructions.InsertRange(0, new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, lungeField),
            new(OpCodes.Callvirt, PropertyGetter(typeof(KeySubroutine<Scp939Role>), nameof(KeySubroutine<Scp939Role>.Owner))),
            new(OpCodes.Stloc_S, ownerLocal.LocalIndex)
        });

        // Index, after load current collider from Scp939Motor.Detections in for cycle
        int targetIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldelem_Ref) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                // Duplicate current collider to stack
                new(OpCodes.Dup),
                // Load owner
                new(OpCodes.Ldloc_S, ownerLocal.LocalIndex),
                // load this._lunge
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, lungeField),
                // invoke HandleDetection(Scp939Motor.Detections[i], owner, this._lunge)
                new(OpCodes.Call, Method(typeof(MotorOverlapCapsulePatch), nameof(HandleDetection)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Release(newInstructions);
    }
}
