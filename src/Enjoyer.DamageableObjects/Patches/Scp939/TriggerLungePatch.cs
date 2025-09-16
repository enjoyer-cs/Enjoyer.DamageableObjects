using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;

namespace Enjoyer.DamageableObjects.Patches.Scp939;

/// <summary>
///     Патч метода <see cref="Scp939LungeAbility.TriggerLunge" />, нужен для очистки
///     <see cref="MotorOverlapCapsulePatch._processedComponents" />
/// </summary>
[HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.TriggerLunge))]
internal static class TriggerLungePatch
{
    /// <summary>
    ///     Метод, вызываемый после исполнения <see cref="Scp939LungeAbility.TriggerLunge" />,
    ///     очищает <see cref="MotorOverlapCapsulePatch._processedComponents" />
    /// </summary>
    private static void Postfix(Scp939LungeAbility __instance) =>
        MotorOverlapCapsulePatch._processedComponents.Remove(__instance);
}