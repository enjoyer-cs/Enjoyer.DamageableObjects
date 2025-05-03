using AdminToys;
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
    private const int WindowLayer = 14;

    /// <summary>
    ///     Получает или задаёт максимальное здоровье объекта.
    /// </summary>
    public virtual uint MaxHealth { get; set; }

    /// <summary>
    ///     Получает или задаёт эффективность защиты против урона, зависящего от неё.
    /// </summary>
    public virtual int ProtectionEfficacy { get; set; }

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
    ///     Дочерние объекты <see cref="Component.gameObject"/>
    /// </summary>
    public IReadOnlyCollection<GameObject>? ChildrenObjects { get; set; }

    protected virtual void Start()
    {
        Health = MaxHealth;

        if (ChildrenObjects != null)
            foreach (GameObject child in ChildrenObjects)
                child.gameObject.layer = WindowLayer;

        gameObject.layer = WindowLayer;

        SubscribeEvents();
    }

    protected virtual void OnDestroy() => UnsubscribeEvents();

    protected virtual void SubscribeEvents()
    {
        MapEvents.ExplodingGrenade += OnExploding;
        PlayerEvents.Shot += OnShot;
        Scp939Events.Clawed += OnClawed;
    }

    protected virtual void UnsubscribeEvents()
    {
        MapEvents.ExplodingGrenade -= OnExploding;
        PlayerEvents.Shot -= OnShot;
        Scp939Events.Clawed -= OnClawed;
    }

    protected virtual void OnShot(ShotEventArgs ev)
    {
        if (!CheckRaycastHit(ev.RaycastHit)) return;

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

    protected internal virtual void OnLunging(Player player, Scp939LungeAbility lunge, bool isMainTarget)
    {
        if (isMainTarget)
            ProcessDamage(player, CalculateDamage(ProtectionEfficacy, Scp939LungeAbility.LungeDamage, Scp939ClawAbility.DamagePenetration));
        else
        {
            ProcessDamage(player,
                CalculateDamage(ProtectionEfficacy, Scp939LungeAbility.SecondaryDamage, Scp939ClawAbility.DamagePenetration),
                Scp939LungeAbility.SecondaryHitmarkerSize);
        }

        lunge.State = Scp939LungeState.LandHarsh;
    }

    protected virtual void ProcessDamage(Player? player, float damage, float hitMarkerSize = 1f)
    {
        if (damage <= 0) return;

        Log.Debug($"[ProcessDamage] Health: {Health}, damage: {damage}");

        Health -= damage;

        if (hitMarkerSize > 0) player?.ShowHitMarker();
        if (Health > 0) return;

        Health = 0;
        Destroy(gameObject);
        OnDestroyedByDamage?.Invoke(gameObject, player);
    }

    private static float CalculateDamage(int efficacy, float damage, float armorPenetration) =>
        CalculateDamage(efficacy, damage, Mathf.RoundToInt(armorPenetration * 100));

    private static float CalculateDamage(int efficacy, float damage, int armorPenetrationPercent) =>
        BodyArmorUtils.ProcessDamage(efficacy, damage, armorPenetrationPercent);

    private bool CheckRaycastHit(Transform originTransform, float maxDistance = 2f) =>
        Physics.Raycast(originTransform.position, originTransform.forward, out RaycastHit hit, maxDistance) &&
        CheckRaycastHit(hit);

    private bool CheckRaycastHit(RaycastHit hit) => this == hit.transform.GetComponentInParent<DamageableComponent>();
}
