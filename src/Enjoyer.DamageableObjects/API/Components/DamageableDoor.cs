using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
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
        Door.IgnoreDamageSources = _doorIgnoredDamage;
        base.Start();
    }

    /// <inheritdoc />
    protected internal override void OnShot(ShotArgs args)
    {
        if (args.Firearm.ItemTypeId is ItemType.ParticleDisruptor && !_doorIgnoredDamage.HasFlag(DoorDamageType.ParticleDisruptor))
            return;

        base.OnShot(args);
    }

    /// <inheritdoc />
    protected override void OnExplosionSpawned(ExplosionSpawnedEventArgs ev)
    {
        if (_doorIgnoredDamage.HasFlag(DoorDamageType.Grenade))
            base.OnExplosionSpawned(ev);
    }

    /// <inheritdoc />
    protected internal override bool OnScp939Lunging(ReferenceHub player, Scp939LungeAbility lunge, bool isMainTarget) =>
        Door.ExactState == 0 && base.OnScp939Lunging(player, lunge, isMainTarget);

    /// <inheritdoc />
    protected internal override void OnScp096Attacking(ReferenceHub? player)
    {
    }

    /// <inheritdoc />
    protected internal override bool OnScp096Charging(ReferenceHub? hub, bool isMainTarget) => true;

    /// <inheritdoc />
    protected override void ProcessDamage(ReferenceHub? damageDealer, float damage, float hitMarkerSize = 1f)
    {
        hitMarkerSize = HitMarkerSize;
        if (Door.IsDestroyed) return;

        base.ProcessDamage(damageDealer, damage, hitMarkerSize);
    }

    /// <inheritdoc />
    protected override void DestroyByDamage(ReferenceHub? destroyer)
    {
        Health = MaxHealth;
        Door.TryBreak();
        OnDestroyedByDamage?.Invoke(gameObject, destroyer);
    }
}