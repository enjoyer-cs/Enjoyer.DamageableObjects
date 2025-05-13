using Exiled.API.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs.Interfaces;

public interface IDoProperties
{
    public int DamageResistance { get; set; }

    public uint MaxHealth { get; set; }

    [Description("""
                 DamageTypes that can be used to deal damage to an object, if empty, the object will be able to take damage from any damage sources.
                 Can be: any firearm DamageType, Firearm, MicroHid, Explosion, Scp018, Scp0492, Scp096, Scp3114, Scp939.
                 """)]
    public List<DamageType>? AllowedDamageTypes { get; set; }

    public Dictionary<DamageType, float> DamageMultipliers { get; set; }
}
