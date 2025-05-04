using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.ThrowableProjectiles;
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

    private static readonly IReadOnlyCollection<DamageType> _firearmDamageTypes =
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
        if (AllowedDamageTypes?.Contains(DamageType.Explosion) != false)
            MapEvents.ExplodingGrenade += OnExploding;

        if (AllowedDamageTypes?.Any(_firearmDamageTypes.Contains) != false)
            PlayerEvents.Shot += OnShot;

        if (AllowedDamageTypes?.Contains(DamageType.Scp939) != false)
            Scp939Events.Clawed += OnClawed;
    }

    protected virtual void UnsubscribeEvents()
    {
        MapEvents.ExplodingGrenade -= OnExploding;
        PlayerEvents.Shot -= OnShot;
        Scp939Events.Clawed -= OnClawed;
    }

    protected virtual void ProcessDamage(Player? player, float damage, float hitMarkerSize = 1f)
    {
        if (damage <= 0) return;

        Log.Debug($"[ProcessDamage] Health: {Health}, damage: {damage}");

        Health -= damage;

        if (hitMarkerSize > 0) player?.ShowHitMarker();
        if (Health > 0) return;

        Health = 0;
        DestroyByDamage(player);
    }

    protected virtual void DestroyByDamage(Player? player)
    {
        Destroy(gameObject);
        OnDestroyedByDamage?.Invoke(gameObject, player);
    }

    public static float CalculateDamage(int efficacy, float damage, float armorPenetration) =>
        CalculateDamage(efficacy, damage, Mathf.RoundToInt(armorPenetration * 100));

    public static float CalculateDamage(int efficacy, float damage, int armorPenetrationPercent) =>
        BodyArmorUtils.ProcessDamage(efficacy, damage, armorPenetrationPercent);

    protected virtual bool CheckRaycastHit(Transform originTransform, float maxDistance = 2f) =>
        Physics.Raycast(originTransform.position, originTransform.forward, out RaycastHit hit, maxDistance) &&
        CheckRaycastHit(hit);

    protected virtual bool CheckRaycastHit(RaycastHit hit) => this == hit.transform.GetComponentInParent<DamageableComponent>();

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
    ///                 <see cref="Player" />
    ///             </term>
    ///             <description>- Игрок, который нанёс финальный урон.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public Action<GameObject, Player?>? OnDestroyedByDamage { get; set; }

    /// <summary>
    ///     Получает или задаёт текущее значение здоровья объекта с этим компонентом.
    /// </summary>
    protected float Health { get; set; }

    /// <summary>
    ///     Дочерние объекты <see cref="Component.gameObject" />
    /// </summary>
    public IReadOnlyCollection<GameObject>? ChildrenObjects { get; set; }

    #endregion

    #region Handlers

    protected virtual void OnShot(ShotEventArgs ev)
    {
        if (!CheckRaycastHit(ev.RaycastHit) || AllowedDamageTypes?.All(type =>
                type is not DamageType.Firearm &&
                type != (DamageType)Enum.Parse(typeof(DamageType), ev.Firearm.FirearmType.ToString(), true)) == true) return;

        HitscanHitregModuleBase hitregModule = ev.Firearm.HitscanHitregModule;
        float baseDamage = hitregModule.DamageAtDistance(ev.Distance);

        Log.Debug($"[OnShot] firearm: {ev.Firearm.FirearmType}, damage: {baseDamage}");

        ProcessDamage(ev.Player, CalculateDamage(ProtectionEfficacy, baseDamage, hitregModule.EffectivePenetration));
    }

    protected virtual void OnExploding(ExplodingGrenadeEventArgs ev)
    {
        if (!ev.IsAllowed || ev.Projectile.Base is not ExplosionGrenade grenade ||
            Physics.Linecast(transform.position, ev.Position, ThrownProjectile.HitBlockerMask))
            return;

        float baseDamage = grenade._playerDamageOverDistance.Evaluate(Vector3.Distance(ev.Position, transform.position));

        Log.Debug($"[{nameof(OnExploding)}] Damage: {baseDamage}]");

        if (baseDamage <= 0) return;

        ProcessDamage(ev.Player, CalculateDamage(ProtectionEfficacy, baseDamage, 50));
    }

    protected virtual void OnClawed(ClawedEventArgs ev)
    {
        if (ev.Scp939.ClawAbility.AttackTriggered || !CheckRaycastHit(ev.Player.CameraTransform)) return;

        ProcessDamage(ev.Player, CalculateDamage(ProtectionEfficacy, Scp939ClawAbility.BaseDamage, Scp939ClawAbility.DamagePenetration));
    }

    protected internal virtual bool OnLunging(Player player, Scp939LungeAbility lunge, bool isMainTarget)
    {
        if (AllowedDamageTypes?.Contains(DamageType.Scp939) == false)
            return false;

        if (isMainTarget)
            ProcessDamage(player, CalculateDamage(ProtectionEfficacy, Scp939LungeAbility.LungeDamage, Scp939ClawAbility.DamagePenetration));
        else
        {
            ProcessDamage(player,
                CalculateDamage(ProtectionEfficacy, Scp939LungeAbility.SecondaryDamage, Scp939ClawAbility.DamagePenetration),
                Scp939LungeAbility.SecondaryHitmarkerSize);
        }

        lunge.State = Scp939LungeState.LandHit;
        lunge._audio.Play(lunge._audio._hits.RandomItem(), 25f);
        return true;
    }

    #endregion
}
