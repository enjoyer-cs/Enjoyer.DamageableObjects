using Enjoyer.DamageableObjects.API.Enums;
using InventorySystem.Items.Armor;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp939;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Scp018Projectile = InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;

namespace Enjoyer.DamageableObjects.API.Components;

/// <summary>
///     Компонент, добавляющий возможность наносить объекту урон и тем самым уничтожать его.
/// </summary>
public class DamageableComponent : MonoBehaviour
{
    private const int _windowLayer = 14;

    public static CachedLayerMask ExplosionBlockerMask { get; } = new("Default", "CCTV", "Door");

    private static IReadOnlyCollection<DamageType> _firearmDamageTypes { get; } =
    [
        DamageType.Firearm,
        DamageType.GunCrossvec,
        DamageType.GunLogicer,
        DamageType.GunRevolver,
        DamageType.GunShotgun,
        DamageType.GunAK,
        DamageType.GunCom15,
        DamageType.GunCom18,
        DamageType.GunFsp9,
        DamageType.GunE11Sr,
        DamageType.ParticleDisruptor,
        DamageType.GunCom45,
        DamageType.GunFrmg0,
        DamageType.GunA7
    ];

    #region Properties

    /// <summary>
    ///     Получает или задаёт максимальное здоровье объекта.
    /// </summary>
    public virtual uint MaxHealth { get; set; }

    /// <summary>
    ///     Получает или задаёт эффективность защиты против урона, зависящего от неё.
    /// </summary>
    public virtual int ProtectionEfficacy { get; set; }

    /// <summary>
    ///     Лист <see cref="DamageType" />, с помощью которых можно наносить урон объекту.<br />
    ///     Если <see langword="null" />, объект будет получать урон от всех <see cref="DamageType" />
    /// </summary>
    public virtual List<DamageType>? AllowedDamageTypes { get; set => field = value?.IsEmpty() is true ? null : value; }

    public virtual Dictionary<DamageType, float> DamageMultipliers { get; set => field = value ?? []; } = [];

    /// <summary>
    ///     Получает или задаёт делегат, вызываемый после нанесения фатального урона объекту.<br />
    /// </summary>
    /// <remarks>
    ///     <list type="number">
    ///         <listheader>
    ///             <term>Передаёт аргументы:</term>
    ///         </listheader>
    ///         <item>
    ///             <term>
    ///                 <see cref="GameObject" />
    ///             </term>
    ///             <description>- Экземпляр уничтоженного объекта.</description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 <see cref="ReferenceHub" />
    ///             </term>
    ///             <description>- Принадлежит игроку, который нанёс финальный урон.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public Action<GameObject, ReferenceHub?>? OnDestroyedByDamage { get; set; }

    /// <summary>
    ///     Получает или задаёт текущее значение здоровья объекта с этим компонентом.
    /// </summary>
    protected float Health { get; set; }

    /// <summary>
    ///     Дочерние объекты <see cref="Component.gameObject" />
    /// </summary>
    public IReadOnlyCollection<GameObject>? ChildrenObjects { get; set; }

    #endregion

    #region Init

    protected virtual void Start()
    {
        Health = MaxHealth;

        if (ChildrenObjects != null)
        {
            foreach (GameObject child in ChildrenObjects)
                child.gameObject.layer = _windowLayer;
        }

        gameObject.layer = _windowLayer;

        SubscribeEvents();
    }

    protected virtual void OnDestroy() => UnsubscribeEvents();

    protected virtual void SubscribeEvents()
    {
        if (IsDamageTypeAllow(DamageType.Explosion))
            ServerEvents.ExplosionSpawned += OnExplosionSpawned;
    }

    protected virtual void UnsubscribeEvents() => ServerEvents.ExplosionSpawned -= OnExplosionSpawned;

    #endregion

    #region General Methods

    protected virtual void ProcessDamage(ReferenceHub? damageDealer, float damage, float hitMarkerSize = 1f)
    {
        if (damage <= 0) return;

        DoPlugin.SendDebug($"[ProcessDamage] Health: {Health}, damage: {damage}");

        Health -= damage;

        if (hitMarkerSize > 0 && damageDealer) Hitmarker.SendHitmarkerDirectly(damageDealer, hitMarkerSize);
        if (Health > 0) return;

        Health = 0;
        DestroyByDamage(damageDealer);
    }

    protected virtual void DestroyByDamage(ReferenceHub? destroyer)
    {
        Destroy(gameObject);
        OnDestroyedByDamage?.Invoke(gameObject, destroyer);
    }

    protected virtual bool CheckRaycastHit(Transform originTransform, float maxDistance = 2f) =>
        Physics.Raycast(originTransform.position, originTransform.forward, out RaycastHit hit, maxDistance, gameObject.layer) &&
        CheckRaycastHit(hit);

    protected virtual bool CheckRaycastHit(RaycastHit hit) => this == hit.transform.GetComponentInParent<DamageableComponent>();

    protected virtual float CalculateDamage(int efficacy, float damage, float armorPenetration) =>
        CalculateDamage(efficacy, damage, Mathf.RoundToInt(armorPenetration * 100));

    protected virtual float CalculateDamage(int efficacy, float damage, int armorPenetrationPercent) =>
        BodyArmorUtils.ProcessDamage(efficacy, damage, armorPenetrationPercent);

    public float GetDamageMultiplier(ReferenceHub? player, DamageType damageType)
    {
        if (_firearmDamageTypes.Contains(damageType) && !DamageMultipliers.ContainsKey(damageType))
            return DamageMultipliers.TryGetValue(DamageType.Firearm, out float multiplier) ? multiplier : 1f;

        return DamageMultipliers.TryGetValue(damageType, out float value) ? value : 1f;
    }

    public virtual bool IsDamageTypeAllow(DamageType damageType) => AllowedDamageTypes?.Contains(damageType) != false;

    public virtual bool IsDamageTypeAllow(params IEnumerable<DamageType> damageTypes) =>
        AllowedDamageTypes?.Any(damageTypes.Contains) != false;

    #endregion

    #region Handlers

    protected internal virtual void OnShot(ShotArgs args)
    {
        DoPlugin.SendDebug("Handle shot");

        var firearmDamageType = (DamageType)Enum.Parse(typeof(DamageType), args.Firearm.ItemTypeId.ToString(), true);
        DamageType? damageType = AllowedDamageTypes?.FirstOrDefault(type => type == firearmDamageType || type is not DamageType.Firearm);

        if (AllowedDamageTypes is not null && damageType is null) return;

        float damage = CalculateShotDamage(args, firearmDamageType);

        ProcessDamage(args.Player.ReferenceHub, damage);
    }

    protected virtual float CalculateShotDamage(ShotArgs args, DamageType damageType)
    {
        float baseDamage = args.HitscanHitregModule.DamageAtDistance(args.Distance) *
                           GetDamageMultiplier(args.Player.ReferenceHub, damageType);

        DoPlugin.SendDebug($"[{nameof(OnShot)}] Firearm: {args.Firearm.ItemTypeId}, Damage: {baseDamage}");

        return CalculateDamage(ProtectionEfficacy, baseDamage, args.HitscanHitregModule.EffectivePenetration);
    }

    protected internal virtual bool OnDisruptorSingleShot(ReferenceHub? player, float damage)
    {
        DoPlugin.SendDebug($"[{nameof(OnDisruptorSingleShot)}] Trying to handle shot");

        if (!IsDamageTypeAllow(DamageType.ParticleDisruptor, DamageType.Firearm))
            return false;

        damage = CalculateDisruptorDamage(player, damage);
        ProcessDamage(player, damage);
        return true;
    }

    protected virtual float CalculateDisruptorDamage(ReferenceHub? player, float damage) =>
        damage * GetDamageMultiplier(player, DamageType.ParticleDisruptor);

    protected virtual void OnExplosionSpawned(ExplosionSpawnedEventArgs ev)
    {
        DoPlugin.SendDebug($"[{nameof(OnExplosionSpawned)}] Trying to handle Explosion");

        if (Physics.Linecast(transform.position, ev.Position, ExplosionBlockerMask)) return;

        float damage = CalculateExplosionDamage(ev);

        if (damage <= 0)
        {
            DoPlugin.SendDebug($"[{nameof(OnExplosionSpawned)}] Base damage equals 0");
            return;
        }

        ProcessDamage(ev.Player?.ReferenceHub, damage);
    }

    protected virtual float CalculateExplosionDamage(ExplosionSpawnedEventArgs ev)
    {
        float baseDamage = ev.Settings._playerDamageOverDistance.Evaluate(Vector3.Distance(ev.Position, transform.position)) *
                           GetDamageMultiplier(ev.Player?.ReferenceHub, DamageType.Explosion);

        return CalculateDamage(ProtectionEfficacy, baseDamage, 50);
    }

    protected internal void OnScp018Bounce(Scp018Projectile scp018, ReferenceHub? previousOwner)
    {
        DoPlugin.SendDebug($"[{nameof(OnScp018Bounce)}] Trying to handle SCP 018 Bounce");

        if (IsDamageTypeAllow(DamageType.Scp018))
            ProcessDamage(previousOwner, Calculate018Damage(scp018, previousOwner), 0f);
    }

    protected float Calculate018Damage(Scp018Projectile scp018, ReferenceHub? previousOwner) =>
        scp018.CurrentDamage * GetDamageMultiplier(previousOwner, DamageType.Scp018);

    protected internal virtual void OnClawed(ReferenceHub hub)
    {
        DoPlugin.SendDebug($"[{nameof(OnClawed)}] Trying to handle SCP 939 Claw.");

        if (!IsDamageTypeAllow(DamageType.Scp939)) return;

        ProcessDamage(hub, CalculateDamage(ProtectionEfficacy,
            Scp939ClawAbility.BaseDamage * GetDamageMultiplier(hub, DamageType.Scp939), Scp939ClawAbility.DamagePenetration));
    }

    protected internal virtual bool OnLunging(ReferenceHub hub, Scp939LungeAbility lunge, bool isMainTarget)
    {
        DoPlugin.SendDebug($"[{nameof(OnLunging)}] Trying to handle SCP 939 Lunge");

        if (!IsDamageTypeAllow(DamageType.Scp939))
            return false;

        if (!isMainTarget)
        {
            ProcessDamage(hub, CalculateDamage(ProtectionEfficacy,
                Scp939LungeAbility.SecondaryDamage * GetDamageMultiplier(hub, DamageType.Scp939),
                Scp939ClawAbility.DamagePenetration), Scp939LungeAbility.SecondaryHitmarkerSize);
            return true;
        }

        ProcessDamage(hub, CalculateDamage(ProtectionEfficacy,
            Scp939LungeAbility.LungeDamage * GetDamageMultiplier(hub, DamageType.Scp939), Scp939ClawAbility.DamagePenetration));

        lunge.State = Scp939LungeState.LandHit;
        lunge._audio.Play(lunge._audio._hits.RandomItem(), 25f);
        return true;
    }

    protected internal virtual void OnScp096Attacking(ReferenceHub hub)
    {
        DoPlugin.SendDebug($"[{nameof(OnScp096Attacking)}] Trying to handle SCP 096 Attack.");

        if (!IsDamageTypeAllow(DamageType.Scp096)) return;

        ProcessDamage(hub, CalculateScp096Attacked(hub));
    }

    protected virtual float CalculateScp096Attacked(ReferenceHub? player) =>
        Scp096AttackAbility.HumanDamage * GetDamageMultiplier(player, DamageType.Scp096);

    protected internal virtual bool OnCharging(ReferenceHub? player, bool isMainTarget)
    {
        DoPlugin.SendDebug($"[{nameof(OnCharging)}] Trying to handle SCP 096 Charge");

        if (!IsDamageTypeAllow(DamageType.Scp096)) return false;

        ProcessDamage(player, CalculateScp096Charge(player, isMainTarget));
        return true;
    }

    protected virtual float CalculateScp096Charge(ReferenceHub? player, bool isMainTarget)
    {
        float damage = isMainTarget ? Scp096ChargeAbility.DamageTarget : Scp096ChargeAbility.DamageNonTarget;
        return damage * GetDamageMultiplier(player, DamageType.Scp096);
    }

    protected internal virtual void OnScp3114Slaped(ReferenceHub hub)
    {
        DoPlugin.SendDebug($"[{nameof(OnScp3114Slaped)}] Trying to handle SCP 3114 Slap");

        if (!IsDamageTypeAllow(DamageType.Scp3114)) return;

        ProcessDamage(hub, CalculateScp3114Slaped(hub));
    }

    protected virtual float CalculateScp3114Slaped(ReferenceHub? player) => 15 * GetDamageMultiplier(player, DamageType.Scp3114);

    protected internal virtual void OnScp0492Attacked(ReferenceHub hub)
    {
        DoPlugin.SendDebug($"[{nameof(OnScp0492Attacked)}] Trying to handle SCP 0492 Slap");

        if (!IsDamageTypeAllow(DamageType.Scp0492)) return;

        ProcessDamage(hub, CalculateScp0492Attacked(hub));
    }

    protected virtual float CalculateScp0492Attacked(ReferenceHub? player) => 40 * GetDamageMultiplier(player, DamageType.Scp0492);

    #endregion
}