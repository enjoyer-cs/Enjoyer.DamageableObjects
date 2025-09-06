using Enjoyer.DamageableObjects.API.Enums;
using Enjoyer.DamageableObjects.Configs.Interfaces;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public struct DamageableDoorsProperties : IDoProperties
{
    public DamageableDoorsProperties(int damageResistance, uint maxHealth, DoorDamageType notAffectToDamage = DoorDamageType.None,
        Dictionary<DamageType, float>? damageMultipliers = null, params List<DamageType>? allowedDamageSources) : this()
    {
        MaxHealth = maxHealth;
        DamageResistance = damageResistance;
        NotAffectToDamage = notAffectToDamage;
        DamageMultipliers = damageMultipliers ?? [];
        AllowedDamageTypes = allowedDamageSources;
    }

    /// <inheritdoc />
    public uint MaxHealth { get; set; }

    /// <inheritdoc />
    [Description("Damage Resistance in percent for damage, that has penetration")]
    public int DamageResistance { get; set; }

    [Description("DoorDamageType that not could be modified")]
    public DoorDamageType NotAffectToDamage { get; set; }

    /// <inheritdoc />
    [Description(
        "DamageTypes that can be used to deal damage to an object, if empty, the object will be able to take damage from any damage sources.")]
    public List<DamageType>? AllowedDamageTypes { get; set; }

    /// <inheritdoc />
    public Dictionary<DamageType, float> DamageMultipliers { get; set; }
}
