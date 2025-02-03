using MareLib;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Obelisks;

public class ObeliskTeleportGui : Gui
{
    public BlockEntityObelisk obelisk;
    public List<WaystoneData> waystoneData;

    public ObeliskTeleportGui(BlockEntityObelisk obelisk, List<WaystoneData> waystoneData)
    {
        this.obelisk = obelisk;
        this.waystoneData = waystoneData;
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Fixed(0, 0, 700, 700).Alignment(Align.Center).NoScaling();

        Vector2d playerPos = new(MainAPI.Capi.World.Player.Entity.Pos.X, MainAPI.Capi.World.Player.Entity.Pos.Z);

        WidgetObeliskTranslocator obeliskTranslocator = new(this, mainBounds, playerPos, waystoneData, new GridPos(obelisk.Pos.X, obelisk.Pos.Y, obelisk.Pos.Z));

        AddWidget(new ClipWidget(this, true, mainBounds));

        AddWidget(obeliskTranslocator);

        AddWidget(new ClipWidget(this, false, mainBounds));
    }
}