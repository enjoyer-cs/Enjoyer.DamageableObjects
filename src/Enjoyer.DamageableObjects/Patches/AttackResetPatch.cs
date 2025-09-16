using Enjoyer.DamageableObjects.API.Components;
using HarmonyLib;
using PlayerRoles.PlayableScps.Subroutines;
using PlayerRoles.Subroutines;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enjoyer.DamageableObjects.Patches;

[HarmonyPatch]
internal static class AttackResetPatch
{
    internal static Dictionary<object, List<DamageableComponent>> _damagedComponents { get; } = [];

    private static IEnumerable<MethodBase> TargetMethods() =>
        ScpAttackPatch._targetTypes.Select(targetType => AccessTools.Method(targetType, nameof(ScpAttackAbilityBase<>.ServerProcessCmd)));

    private static void Prefix(SubroutineBase __instance) => _damagedComponents[__instance.Role] = [];
}