using MareLib;
using OpenTK.Mathematics;
using System;
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
[Rune("game:ore-olivine")]
public class RuneOfTheCornucopia : Inscription
{
    public override int MaxPower => 50;
    public override Vector4 Color => new(0, 1, 0, 1);
    private long listenerId;

    private static float PotentiaPerSatiety => 1;

    private static readonly SimpleParticleProperties liquidParticles;
    static RuneOfTheCornucopia()
    {
        liquidParticles = new SimpleParticleProperties()
        {
            MinVelocity = new Vec3f(0, 5, 0),
            AddVelocity = new Vec3f(0, 5, 0),
            addLifeLength = 3f,
            LifeLength = 1f,
            MinQuantity = 2,
            AddQuantity = 2,
            GravityEffect = 2f,
            SelfPropelled = false,
            MinSize = 0.35f,
            MaxSize = 1f,
            Color = ColorUtil.ToRgba(100, 0, 200, 0)
        };
    }

    public RuneOfTheCornucopia(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void OnAdded()
    {
        if (isServer)
        {
            listenerId = MainAPI.Sapi.Event.RegisterGameTickListener(CheckNearbyVessels, 30000);
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

        Random rand = new();

        int foodToConsume = (int)(rand.Next(5) * PowerPercent);

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

                for (int i = 0; i < 10; i++)
                {
                    liquidParticles.MinPos = new Vec3d(x, y + 1f, z);
                    liquidParticles.AddPos = new Vec3d(0.8f, 0, 0.8f);

                    MainAPI.Sapi.World.SpawnParticles(liquidParticles);
                    liquidParticles.Color = ColorUtil.ToRgba(255, rand.Next(255), rand.Next(255), rand.Next(255));
                }

                foodToConsume--;
                if (foodToConsume == 0) return;
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