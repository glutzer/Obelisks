using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Rune("game:ore-corundum")]
public class RuneOfPower : Inscription
{
    public override int MaxPower => 100;
    public override Vector4 Color => new(1, 0.4f, 0, 1);

    public RuneOfPower(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void ModifyStats(ObeliskStats stats)
    {
        stats.powerMultiplier *= 1 + PowerPercent;
    }
}