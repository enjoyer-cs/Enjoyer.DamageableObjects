using PlayerRoles.PlayableScps.Scp096;
using System;

namespace Enjoyer.DamageableObjects.Events.Scp;

public static class Scp096Events
{
    public static Action<Scp096AttackAbility>? Attacked { get; set; }

    public static void OnAttacked(Scp096AttackAbility ability) => Attacked?.Invoke(ability);
}
