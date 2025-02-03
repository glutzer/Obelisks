using MareLib;
using OpenTK.Mathematics;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Obelisks;

/// <summary>
/// Takes food from nearby 
/// </summary>
[Rune("game:gem-emerald-rough")]
public class RuneOfTheCornucopia : Inscription
{
    public override int MaxPower => 10;
    public override Vector4 Color => new(0, 1, 0, 1);
    private long listenerId;

    private static float PotentiaPerSatiety => 0.1f;

    public RuneOfTheCornucopia(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void OnAdded()
    {
        if (isServer)
        {
            listenerId = MainAPI.Sapi.Event.RegisterGameTickListener(CheckNearbyVessels, 5000);
        }
    }

    public void CheckNearbyVessels(float dt)
    {
        MainAPI.Sapi.World.BlockAccessor.WalkBlocks(obelisk.Pos.SubCopy(1, 1, 1), obelisk.Pos.AddCopy(1, 1, 1), CheckBlock);
    }

    public void CheckBlock(Block block, int x, int y, int z)
    {
        if (block is not BlockGenericTypedContainer) return;

        if (MainAPI.Sapi.World.AllPlayers.First() is not IServerPlayer player || player.Entity == null) return;

        if (MainAPI.Sapi.World.BlockAccessor.GetBlockEntity(new BlockPos(x, y, z)) is not BlockEntityGenericTypedContainer be) return;

        foreach (ItemSlot slot in be.Inventory)
        {
            if (slot.Itemstack == null) continue;

            FoodNutritionProperties? nutrition = slot.Itemstack.Collectible.GetNutritionProperties(MainAPI.Server, slot.Itemstack, player.Entity);
            if (nutrition == null) continue;

            if (nutrition.Satiety > 0)
            {
                TransitionState? state = slot.Itemstack.Collectible.UpdateAndGetTransitionState(MainAPI.Server, slot, EnumTransitionType.Perish);
                float spoilLevel = state?.TransitionLevel ?? 0;
                float satLoss = GlobalConstants.FoodSpoilageSatLossMul(spoilLevel, slot.Itemstack, player.Entity);
                float totalSatiety = nutrition.Satiety * satLoss;
                slot.TakeOut(1);
                slot.MarkDirty();
                obelisk.stats.AddPotentia((int)(totalSatiety * PotentiaPerSatiety));
                obelisk.MarkDirty();
                return;
            }
        }
    }

    public override void OnRemoved()
    {
        if (isServer)
        {
            MainAPI.Sapi.Event.UnregisterGameTickListener(listenerId);
            listenerId = 0;
        }
    }
}