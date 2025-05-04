﻿using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.PlayableScps.Scp939;

namespace Enjoyer.DamageableObjects.API.Components;

public sealed class DamageableDoor : DamageableComponent
{
    private DoorDamageType _doorIgnoredDamage =>
        (DoorDamageType.Grenade | DoorDamageType.Weapon | DoorDamageType.Scp096 | DoorDamageType.ParticleDisruptor) ^ NotAffectToDamage;

    public DoorDamageType NotAffectToDamage { get; set; }

    public BreakableDoor Door { get; set; } = null!;

    public float HitMarkerSize { get; set; }

    /// <inheritdoc />
    protected override void Start()
    {
        Door.IgnoredDamage = _doorIgnoredDamage;
        base.Start();
    }

    /// <inheritdoc />
    protected internal override bool OnLunging(Player player, Scp939LungeAbility lunge, bool isMainTarget) =>
        !Door.IsFullyOpen && base.OnLunging(player, lunge, isMainTarget);

    /// <inheritdoc />
    protected override void ProcessDamage(Player? player, float damage, float hitMarkerSize = 1f)
    {
        hitMarkerSize = HitMarkerSize;

        base.ProcessDamage(player, damage, hitMarkerSize);
    }

    /// <inheritdoc />
    protected override void DestroyByDamage(Player? player)
    {
        Door.Break();
        Destroy(this);
        OnDestroyedByDamage?.Invoke(gameObject, player);
    }
}
