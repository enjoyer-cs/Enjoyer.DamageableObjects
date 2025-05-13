using Footprinting;

namespace Enjoyer.DamageableObjects.API.Methods;

public static class PlayerMethods
{
    public static ReferenceHub? SaveGetHub(Footprint? footprint) => footprint?.Hub;
}
