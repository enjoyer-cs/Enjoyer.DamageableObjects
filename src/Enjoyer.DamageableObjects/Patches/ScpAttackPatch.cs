using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Subroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Pool;
using Logger = LabApi.Features.Console.Logger;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch]
internal static class ScpAttackPatch
{
    internal static Type[] _targetTypes { get; } =
    [
        typeof(ScpAttackAbilityBase<ZombieRole>),
        typeof(ScpAttackAbilityBase<Scp3114Role>),
        typeof(ScpAttackAbilityBase<Scp939Role>)
    ];

    private static IEnumerable<MethodBase> TargetMethods() =>
        _targetTypes.Select(targetType => Method(targetType, nameof(ScpAttackAbilityBase<>.DetectDestructibles)));

    private static void Finalizer(Exception? __exception)
    {
        if (__exception != null) Logger.Error(__exception);
    }

    private static void HandleDetection(Collider detection, ReferenceHub hub)
    {
        try
        {
            PlayerRoleBase hubRole = hub.roleManager.CurrentRole;

            if (!detection.transform.TryGetComponentInParent(out DamageableComponent damageable) ||
                AttackResetPatch._damagedComponents.GetOrAddNew(hubRole).Contains(damageable))
                return;

            AttackResetPatch._damagedComponents[hubRole].Add(damageable);

            switch (hub.roleManager.CurrentRole.RoleTypeId)
            {
                case RoleTypeId.Scp0492:
                    damageable.OnScp0492Attacking(hub);
                    break;
                case RoleTypeId.Scp3114:
                    damageable.OnScp3114Slapping(hub);
                    break;
                case RoleTypeId.Scp939:
                    damageable.OnScp939Clawing(hub);
                    break;
                default:
                    DoPlugin.SendDebug($"Player {hub} hasn't required role");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        ListPool<CodeInstruction>.Get(out List<CodeInstruction> newInstructions);
        newInstructions.AddRange(instructions);

        // Index, after save collider to local in cycle
        CodeInstruction targetInstruction = newInstructions.First(i => i.opcode == OpCodes.Stloc_S);
        int targetIndex = newInstructions.IndexOf(targetInstruction) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_S, targetInstruction.operand),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(ScpAttackPatch), nameof(HandleDetection)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Release(newInstructions);
    }
}