using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;

namespace Enjoyer.DamageableObjects.Patches.Scp096;

[HarmonyPatch(typeof(Scp096AttackAbility), nameof(Scp096AttackAbility.ServerAttack))]
internal static class AttackAbilityPatch
{
    private static void Prefix(Scp096AttackAbility __instance) => ProcessHitsPatch._attackedComponents.Remove(__instance.CastRole);
}
