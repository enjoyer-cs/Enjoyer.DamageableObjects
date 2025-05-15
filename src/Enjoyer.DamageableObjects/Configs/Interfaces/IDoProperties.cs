using Exiled.API.Enums;
using System.Collections.Generic;

namespace Enjoyer.DamageableObjects.Configs.Interfaces;

public interface IDoProperties
{
    public uint MaxHealth { get; set; }

    public int DamageResistance { get; set; }

    public List<DamageType>? AllowedDamageTypes { get; set; }

    public Dictionary<DamageType, float> DamageMultipliers { get; set; }
}
