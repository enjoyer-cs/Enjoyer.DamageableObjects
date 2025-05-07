using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Subroutines;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches.Events.Scp939;

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
    internal static Dictionary<Player, List<DamageableComponent>> _processedComponents { get; } = [];

    private static void HandleDetection(Collider detection, Player player, Scp939LungeAbility lunge)
    {
        try
        {
            List<DamageableComponent>? ignoreComponents = _processedComponents.GetOrAdd(player, () => []);

            if (detection.GetComponentInParent<DamageableComponent>() is not { } damageable ||
                ignoreComponents.Contains(damageable) || !damageable.OnLunging(player, lunge, _processedComponents[player].IsEmpty()))
                return;

            ignoreComponents.Add(damageable);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        LocalBuilder ownerLocal = generator.DeclareLocal(typeof(Player));
        FieldInfo lungeField = Field(typeof(Scp939Motor), nameof(Scp939Motor._lunge));

        // Player owner = Player.Get(this._lunge.Owner)
        newInstructions.InsertRange(0, new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, lungeField),
            new(OpCodes.Callvirt, PropertyGetter(typeof(KeySubroutine<Scp939Role>), nameof(KeySubroutine<Scp939Role>.Owner))),
            new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get),
            [
                typeof(ReferenceHub)
            ])),
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

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
