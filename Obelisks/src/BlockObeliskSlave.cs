using MareLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

/// <summary>
/// Spawned by the block.
/// Slave to bottom block.
/// </summary>
[Block]
public class BlockObeliskSlave : Block
{
    private int height;

    private BlockObelisk masterBlock = null!;

    private Cuboidf[] selBoxes = null!;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        height = int.Parse(Variant["height"]);

        masterBlock = (BlockObelisk)api.World.GetBlock(new AssetLocation("obelisks:obelisk"));

        selBoxes = new Cuboidf[masterBlock.SelectionBoxes.Length];

        for (int i = 0; i < masterBlock.SelectionBoxes.Length; i++)
        {
            Cuboidf selBox = masterBlock.SelectionBoxes[i].OffsetCopy(0, -height, 0);
            selBoxes[i] = selBox;
        }
    }

    public BlockEntityObelisk? GetBlockEntity(BlockPos pos)
    {
        return api.World.BlockAccessor.GetBlockEntity(pos.AddCopy(0, -height, 0)) as BlockEntityObelisk;
    }

    public BlockObelisk? TryGetMasterBlock(BlockPos pos)
    {
        return api.World.BlockAccessor.GetBlock(pos.AddCopy(0, -height, 0)) as BlockObelisk;
    }

    public BlockPos MasterHeight(BlockPos pos)
    {
        return pos.AddCopy(0, -height, 0);
    }

    public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
    {
        return TryGetMasterBlock(pos)?.GetPlacedBlockInfo(world, MasterHeight(pos), forPlayer) ?? "";
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return TryGetMasterBlock(blockSel.Position)?.OnBlockInteractStart(world, byPlayer, blockSel.AddPosCopy(0, -height, 0)) ?? false;
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return TryGetMasterBlock(blockSel.Position)?.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel.AddPosCopy(0, -height, 0)) ?? false;
    }

    public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
    {
        return TryGetMasterBlock(blockSel.Position)?.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel.AddPosCopy(0, -height, 0), cancelReason) ?? false;
    }

    public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        TryGetMasterBlock(blockSel.Position)?.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel.AddPosCopy(0, -height, 0));
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

        if (api.Side == EnumAppSide.Server && api.World.BlockAccessor.GetBlock(MasterHeight(pos)) == masterBlock)
        {
            world.BlockAccessor.BreakBlock(MasterHeight(pos), byPlayer);
        }
    }

    public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
    {
        return masterBlock?.GetRandomColor(capi, MasterHeight(pos), facing, rndIndex) ?? 0xFFFFFF;
    }

    public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
    {
        return TryGetMasterBlock(pos)?.GetPlacedBlockName(world, MasterHeight(pos)) ?? "";
    }

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return selBoxes;
    }
}