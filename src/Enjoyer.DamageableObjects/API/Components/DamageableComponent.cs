using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.ThrowableProjectiles;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp939;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MapEvents = Exiled.Events.Handlers.Map;
using Scp939Events = Exiled.Events.Handlers.Scp939;
using PlayerEvents = Exiled.Events.Handlers.Player;

namespace Enjoyer.DamageableObjects.API.Components;

/// <summary>
///     Компонент, добавляющий возможность наносить объекту урон и тем самым уничтожать его.
/// </summary>
public class DamageableComponent : MonoBehaviour
{
    private const int _windowLayer = 14;

    private static IReadOnlyCollection<DamageType> _firearmDamageTypes { get; } =
    [
        DamageType.Firearm,
        DamageType.Crossvec,
        DamageType.Logicer,
        DamageType.Revolver,
        DamageType.Shotgun,
        DamageType.AK,
        DamageType.Com15,
        DamageType.Com18,
        DamageType.Fsp9,
        DamageType.E11Sr,
        DamageType.ParticleDisruptor,
        DamageType.Com45,
        DamageType.Frmg0,
        DamageType.A7
    ];

    public static CachedLayerMask ExplosionBlockerMask { get; } = new("Default", "CCTV", "Door");

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
            MapEvents.ExplodingGrenade += OnExploding;

        if (IsDamageTypeAllow(_firearmDamageTypes))
            PlayerEvents.Shot += OnShot;

        if (IsDamageTypeAllow(DamageType.Scp939, DamageType.Scp))
            Scp939Events.Clawed += OnClawed;
    }

    protected virtual void UnsubscribeEvents()
    {
        MapEvents.ExplodingGrenade -= OnExploding;
        PlayerEvents.Shot -= OnShot;
        Scp939Events.Clawed -= OnClawed;
    }

    #endregion

    #region General Methods

    protected virtual void ProcessDamage(ReferenceHub? damageDealer, float damage, float hitMarkerSize = 1f)
    {
        if (damage <= 0) return;

        Log.Debug($"[ProcessDamage] Health: {Health}, damage: {damage}");

        Health -= damage;

        if (hitMarkerSize > 0 && damageDealer is not null) Hitmarker.SendHitmarkerDirectly(damageDealer, hitMarkerSize);
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
        Physics.Raycast(originTransform.position, originTransform.forward, out RaycastHit hit) &&
        CheckRaycastHit(hit) &&
        hit.transform.GetComponentInParent<Collider>() is { } collider &&
        Vector3.Distance(collider.ClosestPoint(originTransform.position), originTransform.position) <= maxDistance;

    protected virtual bool CheckRaycastHit(RaycastHit hit) => this == hit.transform.GetComponentInParent<DamageableComponent>();

    public static float CalculateDamage(int efficacy, float damage, float armorPenetration) =>
        CalculateDamage(efficacy, damage, Mathf.RoundToInt(armorPenetration * 100));

    public static float CalculateDamage(int efficacy, float damage, int armorPenetrationPercent) =>
        BodyArmorUtils.ProcessDamage(efficacy, damage, armorPenetrationPercent);

    public float GetDamageMultiplier(DamageType damageType)
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

    protected virtual void OnShot(ShotEventArgs ev)
    {
        if (ev.Damage > 0)
            ev.Player.ShowHint(ev.Damage.ToString());
        if (!CheckRaycastHit(ev.RaycastHit)) return;

        DamageType firearmDamageType = (DamageType)Enum.Parse(typeof(DamageType), ev.Firearm.FirearmType.ToString(), true);
        DamageType? damageType = AllowedDamageTypes?.FirstOrDefault(type => type == firearmDamageType || type is not DamageType.Firearm);

        if (AllowedDamageTypes is not null && damageType is null) return;

        HitscanHitregModuleBase hitregModule = ev.Firearm.HitscanHitregModule;
        float baseDamage = hitregModule.DamageAtDistance(ev.Distance) * GetDamageMultiplier(firearmDamageType);

        Log.Debug($"[{nameof(OnShot)}] Firearm: {ev.Firearm.FirearmType}, Damage: {baseDamage}");

        ProcessDamage(ev.Player.ReferenceHub, CalculateDamage(ProtectionEfficacy, baseDamage, hitregModule.EffectivePenetration));
    }

    protected internal virtual bool OnDisruptorSingleShot(ReferenceHub? player, float damage)
    {
        Log.Debug($"[{nameof(OnDisruptorSingleShot)}] Trying to handle shot");

        if (!IsDamageTypeAllow(DamageType.ParticleDisruptor, DamageType.Firearm))
            return false;

        damage *= GetDamageMultiplier(DamageType.ParticleDisruptor);
        ProcessDamage(player, damage);
        return true;
    }

    protected virtual void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        Log.Debug($"[{nameof(OnExploding)}] Trying to handle Explosion");

        if (!ev.IsAllowed || ev.Projectile.Base is not ExplosionGrenade grenade ||
            Physics.Linecast(transform.position, ev.Position, ExplosionBlockerMask))
            return;

        float baseDamage = grenade._playerDamageOverDistance.Evaluate(Vector3.Distance(ev.Position, transform.position)) *
                           GetDamageMultiplier(DamageType.Explosion);

        if (baseDamage <= 0)
        {
            Log.Debug($"[{nameof(OnExploding)}] Base damage equals 0");
            return;
        }

        ProcessDamage(ev.Player.ReferenceHub, CalculateDamage(ProtectionEfficacy, baseDamage, 50));
    }

    protected internal void OnScp018Bounce(Scp018Projectile scp018, ReferenceHub? previousOwner)
    {
        if (IsDamageTypeAllow(DamageType.Scp018))
            ProcessDamage(previousOwner, scp018.CurrentDamage, 0f);
    }

    protected virtual void OnClawed(ClawedEventArgs ev)
    {
        if (ev.Scp939.ClawAbility.AttackTriggered || !CheckRaycastHit(ev.Player.CameraTransform)) return;

        Log.Debug($"[{nameof(OnClawed)}] Handle SCP 939 Claw");

        ProcessDamage(ev.Player.ReferenceHub, CalculateDamage(ProtectionEfficacy,
            Scp939ClawAbility.BaseDamage * GetDamageMultiplier(DamageType.Scp939), Scp939ClawAbility.DamagePenetration));
    }

    protected internal virtual bool OnLunging(ReferenceHub player, Scp939LungeAbility lunge, bool isMainTarget)
    {
        Log.Debug($"[{nameof(OnLunging)}] Trying to handle SCP 939 Lunge");

        if (!IsDamageTypeAllow(DamageType.Scp939))
            return false;

        if (!isMainTarget)
        {
            ProcessDamage(player, CalculateDamage(ProtectionEfficacy,
                Scp939LungeAbility.SecondaryDamage * GetDamageMultiplier(DamageType.Scp939),
                Scp939ClawAbility.DamagePenetration), Scp939LungeAbility.SecondaryHitmarkerSize);
            return true;
        }

        ProcessDamage(player, CalculateDamage(ProtectionEfficacy,
            Scp939LungeAbility.LungeDamage * GetDamageMultiplier(DamageType.Scp939), Scp939ClawAbility.DamagePenetration));

        lunge.State = Scp939LungeState.LandHit;
        lunge._audio.Play(lunge._audio._hits.RandomItem(), 25f);
        return true;
    }

    protected internal virtual bool OnScp096Attacking(ReferenceHub? player)
    {
        Log.Debug($"[{nameof(OnScp096Attacking)}] Trying to handle SCP 096 Attack");

        if (!IsDamageTypeAllow(DamageType.Scp096)) return false;

        ProcessDamage(player, Scp096AttackAbility.HumanDamage * GetDamageMultiplier(DamageType.Scp096));
        return true;
    }

    protected internal virtual bool OnCharging(ReferenceHub? player, bool isMainTarget)
    {
        Log.Debug($"[{nameof(OnCharging)}] Trying to handle SCP 096 Charge");

        if (AllowedDamageTypes?.Contains(DamageType.Scp096) == false)
            return false;

        float damage = isMainTarget ? Scp096ChargeAbility.DamageTarget : Scp096ChargeAbility.DamageNonTarget;
        damage *= GetDamageMultiplier(DamageType.Scp096);

        ProcessDamage(player, damage);
        return true;
    }

    #endregion
}
