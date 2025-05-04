using Exiled.API.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public struct DoProperties
{
    public DoProperties(int damageResistance, uint maxHealth, params List<DamageType>? allowedDamageSources) : this()
    {
        DamageResistance = damageResistance;
        MaxHealth = maxHealth;
        AllowedDamageTypes = allowedDamageSources;
    }

    public int DamageResistance { get; set; }

    public uint MaxHealth { get; set; }

    [Description(
        "DamageTypes that can be used to deal damage to an object, if empty, the object will be able to take damage from any damage sources.")]
    public List<DamageType>? AllowedDamageTypes { get; set; }
}
