using Enjoyer.DamageableObjects.API.Enums;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enjoyer.DamageableObjects.Configs;

public sealed class DoConfig
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

    [Description("DoorNames that will be Damageable and FacilityZones where other Doors will be Damageable.")]
    public Dictionary<string, DamageableDoorsProperties> DamageableDoors { get; set; } = new()
    {
        {
            nameof(FacilityZone.Entrance), new DamageableDoorsProperties(40, 500, DoorDamageType.Grenade | DoorDamageType.ParticleDisruptor)
        }
    };

    public float DoorHitMarkerSize { get; set; } = 0.5f;

    public bool Debug { get; set; } = false;
}