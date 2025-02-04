using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Rune("game:nugget-uranium")]
public class RuneOfDelusion : Inscription
{
    public RuneOfDelusion(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override int MaxPower => 200;
    public override Vector4 Color => new(0.5f, 0, 1, 1);

    public override void ModifyStats(ObeliskStats stats)
    {
        stats.maxPotentia = (int)(stats.maxPotentia * (1 + PowerPercent));
    }
}