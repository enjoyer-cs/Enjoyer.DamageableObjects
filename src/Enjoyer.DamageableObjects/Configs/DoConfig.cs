using Enjoyer.DamageableObjects.API.Enums;
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

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool Debug { get; set; } = false;
}
