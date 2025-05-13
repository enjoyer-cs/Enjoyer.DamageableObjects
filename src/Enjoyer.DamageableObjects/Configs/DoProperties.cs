using Enjoyer.DamageableObjects.Configs.Interfaces;
using Exiled.API.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public struct DoProperties : IDoProperties
{
    public DoProperties(int damageResistance, uint maxHealth, Dictionary<DamageType, float>? damageMultipliers = null,
        params List<DamageType>? allowedDamageSources) : this()
    {
        DamageResistance = damageResistance;
        MaxHealth = maxHealth;
        DamageMultipliers = damageMultipliers ?? [];
        AllowedDamageTypes = allowedDamageSources;
    }

    /// <inheritdoc />
    public int DamageResistance { get; set; }

    /// <inheritdoc />
    public uint MaxHealth { get; set; }

    [Description(
        "DamageTypes that can be used to deal damage to an object, if empty, the object will be able to take damage from any damage sources.")]
    public List<DamageType>? AllowedDamageTypes { get; set; }

    /// <inheritdoc />
    public Dictionary<DamageType, float> DamageMultipliers { get; set; }
}
