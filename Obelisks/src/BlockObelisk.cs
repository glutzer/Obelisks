using MareLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Obelisks;

[Block]
public class BlockObelisk : Block
{
    private readonly Block[] slaveBlocks = new Block[2];

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        slaveBlocks[0] = api.World.GetBlock(new AssetLocation("obelisks:obeliskslave-1"));
        slaveBlocks[1] = api.World.GetBlock(new AssetLocation("obelisks:obeliskslave-2"));
    }

    public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref string failureCode)
    {
        BlockPos pos = blockSel.Position.Copy();

        for (int i = 0; i < 2; i++)
        {
            pos.Add(0, 1, 0);
            if (world.BlockAccessor.GetBlock(pos).Id != 0)
            {
                failureCode = "obelisk-block-failure";
                return false;
            }
        }

        return base.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode);
    }

    public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack? byItemStack = null)
    {
        base.OnBlockPlaced(world, blockPos, byItemStack);

        if (api.Side == EnumAppSide.Client) return;

        BlockPos pos = blockPos.Copy();

        for (int i = 0; i < 2; i++)
        {
            pos.Add(0, 1, 0);
            // Place slave block.
            if (world.BlockAccessor.GetBlock(pos).Id == 0)
            {
                world.BlockAccessor.SetBlock(slaveBlocks[i].Id, pos);
            }
            else
            {
                world.BlockAccessor.BreakBlock(blockPos, null);
                break;
            }
        }
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

        if (api.Side == EnumAppSide.Client) return;

        if (world.BlockAccessor.GetBlock(pos) is BlockObelisk) return;

        BlockPos blockPos = pos.Copy();
        for (int i = 0; i < 2; i++)
        {
            blockPos.Add(0, 1, 0);
            Block block = world.BlockAccessor.GetBlock(blockPos);
            if (block is BlockObeliskSlave)
            {
                world.BlockAccessor.BreakBlock(blockPos, byPlayer);
            }
        }
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        // Get block entity.
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityObelisk be)
        {
            if (byPlayer.Entity.Controls.CtrlKey)
            {
                if (api.Side == EnumAppSide.Server)
                {
                    ObeliskSelection selection = (ObeliskSelection)blockSel.SelectionBoxIndex;
                    be.HandleCtrlInteraction((IServerPlayer)byPlayer, selection);
                }

                return true;
            }

            be.currentGlyph?.OnInteract(byPlayer);
            foreach (Inscription? item in be.runes)
            {
                item?.OnInteract(byPlayer);
            }
        }
        return true;
    }
}