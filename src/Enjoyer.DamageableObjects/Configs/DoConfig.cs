using Exiled.API.Enums;
using Exiled.API.Interfaces;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public sealed class DoConfig : IConfig
{
    [Description("Schematics that will be Damageable. Works only with MER.")]
    public Dictionary<string, DoProperties> DamageableSchematics { get; set; } = new()
    {
        {
            "EXAMPLE", new DoProperties(0, 1000, new Dictionary<DamageType, float>
            {
                {
                    DamageType.Scp096, 3.5f
                }
            })
        }
    };

    [Description("DoorTypes that will be Damageable.")]
    public Dictionary<DoorType, DamageableDoorsProperties> DamageableDoorTypes { get; set; } = new()
    {
        {
            DoorType.EntranceDoor, new DamageableDoorsProperties(40, 500, DoorDamageType.Grenade | DoorDamageType.ParticleDisruptor)
        }
    };

    public float DoorHitMarkerSize { get; set; } = 0.5f;

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool Debug { get; set; } = false;
}
