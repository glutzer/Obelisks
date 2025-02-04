using OpenTK.Mathematics;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks.src.runes;

[Rune("game:ore-fluorite")]
public class RuneOfInspiration : Inscription
{
    public override int MaxPower => 50;
    public override Vector4 Color => new(1, 0.8f, 0, 1);

    public RuneOfInspiration(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void ModifyStats(ObeliskStats stats)
    {
        stats.powerMultiplier *= 1 - (0.25f * PowerPercent);
    }
}