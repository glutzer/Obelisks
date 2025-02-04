using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Rune("game:ore-phosphorite")]
public class RuneOfExpansion : Inscription
{
    public override int MaxPower => 10;
    public override Vector4 Color => new(1, 0.9f, 0.8f, 1);

    public RuneOfExpansion(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void ModifyStats(ObeliskStats stats)
    {
        stats.aoeMultiplier *= 1 + PowerPercent;
        stats.powerMultiplier *= 1 + PowerPercent;
    }
}