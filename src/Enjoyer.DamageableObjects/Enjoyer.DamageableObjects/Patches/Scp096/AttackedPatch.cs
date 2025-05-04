using Enjoyer.DamageableObjects.Events.Scp;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp096;

namespace Enjoyer.DamageableObjects.Patches.Scp096;

[HarmonyPatch(typeof(Scp096AttackAbility), nameof(Scp096AttackAbility.ServerProcessCmd))]
internal static class AttackedPatch
{
    private static void Postfix(Scp096AttackAbility __instance) => Scp096Events.OnAttacked(__instance);
}
