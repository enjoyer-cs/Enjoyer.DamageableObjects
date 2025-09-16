using Enjoyer.DamageableObjects.API.Enums;
using Enjoyer.DamageableObjects.Configs.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public struct DoProperties : IDoProperties
{
    public DoProperties(int damageResistance, uint maxHealth, Dictionary<DamageType, float>? damageMultipliers = null,
        params List<DamageType>? allowedDamageSources) : this()
    {
        MaxHealth = maxHealth;
        DamageResistance = damageResistance;
        DamageMultipliers = damageMultipliers ?? [];
        AllowedDamageTypes = allowedDamageSources;
    }

    /// <inheritdoc />
    public uint MaxHealth { get; set; }

    /// <inheritdoc />
    [Description("Damage Resistance in percent for damage, that has penetration")]
    public int DamageResistance { get; set; }

    [Description(
        "DamageTypes that can be used to deal damage to an object, if empty, the object will be able to take damage from any damage sources.")]
    public List<DamageType>? AllowedDamageTypes { get; set; }

    /// <inheritdoc />
    public Dictionary<DamageType, float> DamageMultipliers { get; set; }
}