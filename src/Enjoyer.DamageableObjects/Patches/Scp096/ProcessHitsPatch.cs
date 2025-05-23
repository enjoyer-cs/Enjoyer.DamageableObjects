﻿using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches.Scp096;

[HarmonyPatch(typeof(Scp096HitHandler), nameof(Scp096HitHandler.ProcessHits))]
public class ProcessHitsPatch
{
    /// <summary>
    ///     Компоненты, обработанные при последнем использовании <see cref="Scp096ChargeAbility" />.
    /// </summary>
    internal static Dictionary<Scp096Role, List<DamageableComponent>> _chargeAttackedComponents { get; } = [];

    /// <summary>
    ///     Компоненты, обработанные при последнем использовании <see cref="Scp096AttackAbility" />.
    /// </summary>
    internal static Dictionary<Scp096Role, List<DamageableComponent>> _attackedComponents { get; } = [];

    private static void HandleDetection(Collider detection, ReferenceHub? player, Scp096Role role)
    {
        try
        {
            List<DamageableComponent>? ignoreComponents;
            if (detection.GetComponentInParent<DamageableComponent>() is not { } damageable)
                return;

            switch (role.StateController.AbilityState)
            {
                case Scp096AbilityState.Attacking:
                    ignoreComponents = _attackedComponents.GetOrAdd(role, () => []);

                    if (!ignoreComponents.Contains(damageable) && damageable.OnScp096Attacking(player))
                        ignoreComponents.Add(damageable);
                    break;
                case Scp096AbilityState.Charging:
                    ignoreComponents = _chargeAttackedComponents.GetOrAdd(role, () => []);

                    if (!ignoreComponents.Contains(damageable) && damageable.OnCharging(player, ignoreComponents.IsEmpty()))
                        ignoreComponents.Add(damageable);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        Label handleDetectionLabel = generator.DefineLabel();

        MethodInfo[] methods = typeof(Collider).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        MethodInfo targetMethod = methods.First(method =>
            method.Name == nameof(Collider.TryGetComponent) && method.IsGenericMethod && method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 1);

        // Index, after load current collider from Scp939Motor.Detections in for cycle
        int targetIndex = newInstructions.FindIndex(i =>
            i.opcode == OpCodes.Callvirt && ReferenceEquals(i.operand, targetMethod.MakeGenericMethod(typeof(IDestructible)))) + 1;

        newInstructions[targetIndex].operand = handleDetectionLabel;

        targetIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Blt) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                // Load current collider
                new CodeInstruction(OpCodes.Ldloc_S, 4).WithLabels(handleDetectionLabel),
                // Load hub of owner
                new(OpCodes.Ldloc_2),
                // Load this._scpRole
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(Scp096HitHandler), nameof(Scp096HitHandler._scpRole))),
                // invoke HandleDetection(hit, owner, this._scpRole)
                new(OpCodes.Call, Method(typeof(ProcessHitsPatch), nameof(HandleDetection)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
