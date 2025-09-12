using Enjoyer.DamageableObjects.Configs;
using Enjoyer.DamageableObjects.Handlers;
using HarmonyLib;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using System;

namespace Enjoyer.DamageableObjects;

/// <summary>
///     Основной класс плагина DamageableObjects
/// </summary>
public sealed class DoPlugin : Plugin<DoConfig>
{
    private EventHandlers? _handlers;

    private static bool _isDebugMode => PluginConfig.Debug;

    private Harmony _harmony { get; } = new("damageableobjects.enjoyer-cs");

    /// <summary>
    ///     Экземпляр активного в данный момент <see cref="DoConfig" />
    /// </summary>
    public static DoConfig PluginConfig { get; private set; } = null!;

    /// <inheritdoc />
    public override string Author => "enjoyer-cs";

    /// <inheritdoc />
    public override string Name => "DamageableObjects";

    public override string Description =>
        "Adds support for damageable objects. These objects can be either ProjectMER schematics or standard doors and other GameObjects via modding";

    /// <inheritdoc />
    public override Version Version { get; } = new(ProjectInfo.Version);

    public override Version RequiredApiVersion { get; } = new(1, 1, 1);

    internal static void SendDebug(object? message) => Logger.Debug(message!, _isDebugMode);

    /// <inheritdoc />
    public override void Enable()
    {
#if MER
        _handlers = new MerEventHandlers();
#else
        _handlers = new EventHandlers();
#endif

        _handlers.RegisterEvents();
        _harmony.PatchAll();
    }

    /// <inheritdoc />
    public override void Disable()
    {
        _handlers?.UnregisterEvents();
        _handlers = null;

        _harmony.UnpatchAll();
    }

    public override void LoadConfigs()
    {
        base.LoadConfigs();
        PluginConfig = Config ?? throw new NullReferenceException("Can't load config");
    }
}
