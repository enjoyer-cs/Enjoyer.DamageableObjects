using Enjoyer.DamageableObjects.Configs;
using Exiled.API.Features;
using HarmonyLib;
using System;

namespace Enjoyer.DamageableObjects;

/// <summary>
///     Основной класс плагина DamageableObjects
/// </summary>
public sealed class DoPlugin : Plugin<DoConfig>
{
    private EventHandlers? _handlers;

    private Harmony _harmony { get; } = new("damageableobjects.enjoyer-cs");

    /// <summary>
    ///     Экземпляр активного в данный момент <see cref="DoConfig" />
    /// </summary>
    public static DoConfig PluginConfig { get; private set; } = null!;

    /// <inheritdoc />
    public override string Author => "enjoyer-cs";

    /// <inheritdoc />
    public override string Name => "DamageableObjects";

    /// <inheritdoc />
    public override Version Version { get; } = new(ProjectInfo.Version);

    /// <inheritdoc />
    public override Version RequiredExiledVersion { get; } = new("9.6.0");

    /// <inheritdoc />
    public override void OnEnabled()
    {
        base.OnEnabled();
        PluginConfig = Config;

#if MER
        _handlers = new MerEventHandlers();
#else
        _handlers = new EventHandlers();
#endif

        _handlers.RegisterEvents();
        _harmony.PatchAll();
    }

    /// <inheritdoc />
    public override void OnDisabled()
    {
        base.OnDisabled();

        _handlers?.UnregisterEvents();
        _handlers = null;

        _harmony.UnpatchAll();
    }

    /// <inheritdoc />
    public override void OnReloaded()
    {
        base.OnReloaded();
        PluginConfig = Config;
    }
}
