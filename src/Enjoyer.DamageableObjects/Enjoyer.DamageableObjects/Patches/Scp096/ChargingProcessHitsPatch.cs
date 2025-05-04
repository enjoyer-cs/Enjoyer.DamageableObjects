using Enjoyer.DamageableObjects.API.Components;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Enjoyer.DamageableObjects.Patches.Scp096;

[HarmonyPatch(typeof(Scp096HitHandler), nameof(Scp096HitHandler.ProcessHits))]
public class ChargingProcessHitsPatch
{
    /// <summary>
    ///     Компоненты, обработанные при последнем использовании <see cref="Scp096ChargeAbility" />.
    /// </summary>
    internal static Dictionary<Player, List<DamageableComponent>> _processedComponents { get; } = [];

    private static void HandleDetection(Collider detection, Player player)
    {
        List<DamageableComponent>? ignoreComponents = _processedComponents.GetOrAdd(player, () => []);

        if (detection.GetComponentInParent<DamageableComponent>() is not { } damageable ||
            ignoreComponents.Contains(damageable) || !damageable.OnCharging(player))
            return;

        damageable.OnCharging(player);
        ignoreComponents.Add(damageable);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        LocalBuilder ownerLocal = generator.DeclareLocal(typeof(Player));
        Label handleDetectionLabel = generator.DefineLabel();


        newInstructions.InsertRange(0, new List<CodeInstruction>
        {
            // Player owner = Player.Get(hub)
            new(OpCodes.Ldloc_2),
            new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get),
            [
                typeof(ReferenceHub)
            ])),
            new(OpCodes.Stloc_S, ownerLocal.LocalIndex)
        });

        MethodInfo[] methods = typeof(Collider).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        MethodInfo targetMethod = methods.First(method =>
            method.Name == nameof(Collider.TryGetComponent) && method.IsGenericMethod && method.GetGenericArguments().Length == 1 &&
            method.GetParameters().Length == 1);

        // Index, after load current collider from Scp939Motor.Detections in for cycle
        int targetIndex = newInstructions.FindIndex(i =>
            i.opcode == OpCodes.Callvirt && ReferenceEquals(i.operand, targetMethod.MakeGenericMethod(typeof(IDestructible)))) + 1;

        Log.Warn(targetMethod);

        newInstructions[targetIndex].operand = handleDetectionLabel;

        targetIndex = newInstructions.FindIndex(i => i.opcode == OpCodes.Blt) + 1;

        newInstructions.InsertRange(
            targetIndex, new List<CodeInstruction>
            {
                // Load current collider
                new CodeInstruction(OpCodes.Ldloc_S, 4).WithLabels(handleDetectionLabel),
                // Load owner
                new(OpCodes.Ldloc_S, ownerLocal.LocalIndex),
                // invoke HandleDetection(hit, owner)
                new(OpCodes.Call, Method(typeof(ChargingProcessHitsPatch), nameof(HandleDetection)))
            });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }
}
