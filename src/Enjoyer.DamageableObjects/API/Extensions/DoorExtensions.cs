using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using System.Linq;

namespace Enjoyer.DamageableObjects.API.Extensions;

public static class DoorExtensions
{
    public static string GetDoorNameOrZone(this Door door) =>
        door.DoorName is not DoorName.None
            ? door.DoorName.ToString()
            : door.Base.name.Split(' ').FirstOrDefault() switch
            {
                "LCZ" => nameof(FacilityZone.LightContainment),
                "HCZ" => nameof(FacilityZone.HeavyContainment),
                "EZ" => nameof(FacilityZone.Entrance),
                _ => nameof(DoorName.None)
            };
}