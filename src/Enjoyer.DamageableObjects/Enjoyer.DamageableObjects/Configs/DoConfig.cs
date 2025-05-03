using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace Enjoyer.DamageableObjects.Configs;

public sealed class DoConfig : IConfig
{
    public Dictionary<string, DoProperties> DamageableSchematics { get; set; } = new()
    {
        {
            "EXAMPLE", new DoProperties
            {
                DamageResistance = 0, MaxHealth = 1000
            }
        }
    };

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool Debug { get; set; } = false;
}
