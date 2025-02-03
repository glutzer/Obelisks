using MareLib;
using Obelisks.src.glyphs.bases;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Obelisks;

/// <summary>
/// Gets all players within range, prevents their hunger from going down.
/// </summary>
[Glyph("game:gem-emerald-rough")]
public class GlyphOfStasis : FieldGlyph
{
    public override float FieldRange => PowerPercent * 5 * obelisk.stats.aoeMultiplier;
    public override Vector4 Color => new(0, 0.75f, 0, 1);
    private long listenerId;
    private readonly List<EntityPlayer> playerList = new();

    public GlyphOfStasis(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world, "obelisks:objs/sphere.obj")
    {

    }

    public override void OnAdded()
    {
        base.OnAdded();

        if (isServer)
        {
            MainAPI.Sapi.Event.RegisterGameTickListener(SlowTick, 2000);
        }
    }

    public override void OnRemoved()
    {
        base.OnRemoved();

        if (isServer)
        {
            MainAPI.Sapi.Event.UnregisterGameTickListener(listenerId);
            listenerId = 0;
            foreach (EntityPlayer ePlayer in playerList)
            {
                ePlayer.Stats.Remove("hungerrate", "stasis");
            }
        }
    }

    public void SlowTick(float dt)
    {
        foreach (EntityPlayer ePlayer in playerList)
        {
            ePlayer.Stats.Remove("hungerrate", "stasis");
        }

        playerList.Clear();

        Entity[] entities = MainAPI.Server.GetEntitiesAround(obelisk.Pos.ToVec3d().Add(0.5, 0, 0.5), FieldRange, FieldRange, entity =>
        {
            double dist = entity.Pos.DistanceTo(obelisk.Pos.ToVec3d());
            return dist < FieldRange;
        });

        foreach (Entity ent in entities)
        {
            if (ent is EntityPlayer player)
            {
                playerList.Add(player);
                float current = player.Stats.GetBlended("hungerrate");
                player.Stats.Set("hungerrate", "stasis", -current);
            }
        }
    }
}