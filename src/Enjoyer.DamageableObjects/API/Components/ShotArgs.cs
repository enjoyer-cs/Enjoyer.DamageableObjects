using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;

namespace Enjoyer.DamageableObjects.API.Components;

public sealed class ShotArgs
{
    public ShotArgs(HitscanHitregModuleBase hitregModule, float distance)
    {
        Firearm = hitregModule.Firearm;
        Player = Player.Get(Firearm.Owner);
        HitscanHitregModule = hitregModule;
        Distance = distance;
    }

    public Player Player { get; }

    public Firearm Firearm { get; }

    public HitscanHitregModuleBase HitscanHitregModule { get; }

    public float Distance { get; }
}