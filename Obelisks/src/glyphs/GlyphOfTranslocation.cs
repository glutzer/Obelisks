﻿using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Glyph("game:ore-lapislazuli")]
public class GlyphOfTranslocation : Inscription
{
    public override int MaxPower => 10;
    public override Vector4 Color => new(0, 1, 0.3f, 0.7f);

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