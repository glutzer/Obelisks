using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Glyph("game:ore-lapislazuli")]
public class GlyphOfTranslocation : Inscription
{
    public override int MaxPower => 1;
    public override Vector4 Color => new(0, 1, 0.3f, 0.7f);

    public int PotentiaCost(GridPos targetLocation)
    {
        double distance = pos.DistanceTo(targetLocation.AsBlockPos);
        // 1000 distance should cost 100 potentia before any multipliers.
        double cost = 100 * (distance / 1000);
        cost *= obelisk.stats.potentiaCostMultiplier;
        return (int)cost;
    }

    public GlyphOfTranslocation(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void OnInteract(IPlayer player)
    {
        if (isServer)
        {
            MainAPI.GetGameSystem<TranslocationSystem>(EnumAppSide.Server).OnPlayerClickWaystone(this, player);
        }
    }

    public override void OnCreated()
    {
        if (isServer)
        {
            MainAPI.GetGameSystem<TranslocationSystem>(EnumAppSide.Server).OnWaystoneCreated(this);
        }
    }

    public override void OnDestroyed()
    {
        if (isServer)
        {
            MainAPI.GetGameSystem<TranslocationSystem>(EnumAppSide.Server).OnWaystoneRemoved(this);
        }
    }
}