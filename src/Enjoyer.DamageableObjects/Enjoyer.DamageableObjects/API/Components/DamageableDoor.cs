using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.PlayableScps.Scp939;

namespace Enjoyer.DamageableObjects.API.Components;

public sealed class DamageableDoor : DamageableComponent
{
    private DoorDamageType _doorIgnoredDamage =>
        (DoorDamageType.Grenade | DoorDamageType.Weapon | DoorDamageType.ParticleDisruptor) ^ NotAffectToDamage;

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
    protected override void OnShot(ShotEventArgs ev)
    {
        if (ev.Firearm.Type is ItemType.ParticleDisruptor && !_doorIgnoredDamage.HasFlag(DoorDamageType.ParticleDisruptor))
            return;

        base.OnShot(ev);
    }

    /// <inheritdoc />
    protected override void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (_doorIgnoredDamage.HasFlag(DoorDamageType.Grenade))
            base.OnExploding(ev);
    }

    /// <inheritdoc />
    protected internal override bool OnLunging(ReferenceHub player, Scp939LungeAbility lunge, bool isMainTarget) =>
        !Door.IsFullyOpen && base.OnLunging(player, lunge, isMainTarget);

    /// <inheritdoc />
    protected internal override bool OnScp096Attacking(ReferenceHub? player) => true;

    /// <inheritdoc />
    protected internal override bool OnCharging(ReferenceHub? player, bool isMainTarget) => true;

    /// <inheritdoc />
    protected override void ProcessDamage(ReferenceHub? damageDealer, float damage, float hitMarkerSize = 1f)
    {
        hitMarkerSize = HitMarkerSize;

        if (Door.IsDestroyed)
        {
            Destroy(this);
            return;
        }

        base.ProcessDamage(damageDealer, damage, hitMarkerSize);
    }

    /// <inheritdoc />
    protected override void DestroyByDamage(ReferenceHub? destroyer)
    {
        Door.Break();
        Destroy(this);
        OnDestroyedByDamage?.Invoke(gameObject, destroyer);
    }
}
