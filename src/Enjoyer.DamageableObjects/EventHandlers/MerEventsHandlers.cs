#if MER
using Enjoyer.DamageableObjects.API.Components;
using Enjoyer.DamageableObjects.API.Extensions;
using Enjoyer.DamageableObjects.Configs;
using LabApi.Features.Console;
using ProjectMER.Events.Arguments;
using ProjectMER.Events.Handlers;
using System;
using System.Linq;

namespace Enjoyer.DamageableObjects.EventHandlers;

internal class MerEventHandlers : EventHandlers
{
    /// <inheritdoc />
    internal override void RegisterEvents()
    {
        base.RegisterEvents();
        Schematic.SchematicSpawned += OnSchematicSpawned;
    }

    /// <inheritdoc />
    internal override void UnregisterEvents()
    {
        base.UnregisterEvents();
        Schematic.SchematicSpawned -= OnSchematicSpawned;
    }

    private void OnSchematicSpawned(SchematicSpawnedEventArgs ev)
    {
        Logger.Debug($"Invoked OnSchematicSpawned for schematic with name: {ev.Name}");

        if (DoPlugin.PluginConfig.DamageableSchematics.Keys.FirstOrDefault(ds =>
                string.Equals(ds, ev.Name, StringComparison.CurrentCultureIgnoreCase)) is not { } key ||
            ev.Schematic.gameObject.TryGetComponent(typeof(DamageableComponent), out _))
            return;

        DoProperties props = DoPlugin.PluginConfig.DamageableSchematics[key];

        DamageableComponent? component = ev.Schematic.gameObject.AddDamageableComponent<DamageableComponent>(props);

        component.ChildrenObjects = ev.Schematic.AdminToyBases.Select(toy => toy.gameObject).ToList();
    }
}
#endif
