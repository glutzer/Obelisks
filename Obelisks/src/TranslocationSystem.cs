using MareLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Obelisks;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class WaystoneData
{
    public GridPos position;
    public HashSet<string> discoveredByPlayerUids = new();

    public WaystoneData(GridPos position)
    {
        this.position = position;
    }

    public WaystoneData()
    {

    }
}

public enum WaystoneRequestType
{
    DoTranslocation
}

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class WaystoneRequestPacket
{
    public GridPos gridPos;
    public GridPos fromPos;
    public WaystoneRequestType requestType;

    public WaystoneRequestPacket(GridPos gridPos, GridPos fromPos, WaystoneRequestType requestType)
    {
        this.gridPos = gridPos;
        this.fromPos = fromPos;
        this.requestType = requestType;
    }

    public WaystoneRequestPacket()
    {

    }
}

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class WaystoneGuiPayload
{
    public List<WaystoneData> data;
    public GridPos gridPos;

    public WaystoneGuiPayload(List<WaystoneData> data, GridPos gridPos)
    {
        this.data = data;
        this.gridPos = gridPos;
    }

    public WaystoneGuiPayload()
    {

    }
}

[GameSystem]
public class TranslocationSystem : NetworkedGameSystem
{
    private Dictionary<GridPos, WaystoneData> waystones = new();

    public TranslocationSystem(bool isServer, ICoreAPI api) : base(isServer, api, "obelisktl")
    {
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {
        channel.RegisterMessageType<WaystoneRequestPacket>();
        channel.RegisterMessageType<WaystoneGuiPayload>();

        channel.SetMessageHandler<WaystoneGuiPayload>(OnGuiPacket);
    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {
        channel.RegisterMessageType<WaystoneRequestPacket>();
        channel.RegisterMessageType<WaystoneGuiPayload>();

        channel.SetMessageHandler<WaystoneRequestPacket>(OnMessageReceived);
    }

    /// <summary>
    /// On client receiving packet with every waystone he has discovered.
    /// </summary>
    public static void OnGuiPacket(WaystoneGuiPayload packet)
    {
        BlockPos blockPos = packet.gridPos.AsBlockPos;
        if (MainAPI.Capi.World.BlockAccessor.GetBlockEntity(blockPos) is not BlockEntityObelisk be) return;

        ObeliskTeleportGui gui = new(be, packet.data);
        gui.TryOpen();
    }

    public void OnMessageReceived(IServerPlayer player, WaystoneRequestPacket packet)
    {
        if (packet.requestType == WaystoneRequestType.DoTranslocation)
        {
            if (waystones.TryGetValue(packet.gridPos, out WaystoneData? data) && data.discoveredByPlayerUids.Contains(player.PlayerUID))
            {
                if (MainAPI.Sapi.World.BlockAccessor.GetBlockEntity(packet.fromPos.AsBlockPos) is not BlockEntityObelisk ob) return;
                if (ob.stats.Potentia < 100) return;
                ob.stats.AddPotentia(-100);
                ob.MarkDirty();

                GridPos chunkPos = packet.gridPos / 32;

                // Do translocation.
                MainAPI.Sapi.WorldManager.LoadChunkColumnPriority(chunkPos.X, chunkPos.Z, new ChunkLoadOptions()
                {
                    OnLoaded = () =>
                    {
                        GridPos minPos = packet.gridPos - new GridPos(2, 2, 2);
                        GridPos maxPos = packet.gridPos + new GridPos(2, 2, 2);

                        bool found = false;
                        GridPos newSpawn = packet.gridPos;

                        for (int x = minPos.X; x < maxPos.X; x++)
                        {
                            for (int y = minPos.Y; y < maxPos.Y; y++)
                            {
                                for (int z = minPos.Z; z < maxPos.Z; z++)
                                {
                                    GridPos pos = new(x, y, z);

                                    if (MainAPI.Sapi.World.BlockAccessor.GetBlock(pos.AsBlockPos).Id != 0
                                    && MainAPI.Sapi.World.BlockAccessor.GetBlock(pos.AsBlockPos.Add(0, 1, 0)).Id == 0
                                    && MainAPI.Sapi.World.BlockAccessor.GetBlock(pos.AsBlockPos.Add(0, 2, 0)).Id == 0)
                                    {
                                        newSpawn = new GridPos(x, y + 1, z);
                                        found = true;
                                    }
                                }

                                if (found) break;
                            }

                            if (found) break;
                        }

                        int uses = MainAPI.Sapi.World.Config.GetString("temporalGearRespawnUses", "-1").ToInt();
                        player.SetSpawnPosition(new PlayerSpawnPos()
                        {
                            x = newSpawn.X,
                            y = newSpawn.Y,
                            z = newSpawn.Z,
                            yaw = 0,
                            pitch = 0,
                            RemainingUses = uses
                        });

                        // Kill player.
                        player.Entity.Die(EnumDespawnReason.Combusted, new DamageSource()
                        {

                        });

                        MainAPI.Sapi.SendMessageToGroup(0, $"{player.PlayerName} ascended from flesh.", EnumChatType.Notification);
                    }
                });
            }
        }
    }

    // Server side methods for managing waystones.

    public void OnWaystoneCreated(GlyphOfTranslocation glyph)
    {
        GridPos grid = new(glyph.obelisk.Pos);

        WaystoneData data = new(grid);
        waystones[grid] = data;
    }

    public void OnWaystoneRemoved(GlyphOfTranslocation glyph)
    {
        GridPos grid = new(glyph.obelisk.Pos);
        waystones.Remove(grid);
    }

    public void OnPlayerClickWaystone(GlyphOfTranslocation glyph, IPlayer player)
    {
        GridPos grid = new(glyph.obelisk.Pos);

        if (waystones.TryGetValue(grid, out WaystoneData? data))
        {
            if (data.discoveredByPlayerUids.Add(player.PlayerUID))
            {
                // Waystone discovered.
                MainAPI.Sapi.SendIngameDiscovery((IServerPlayer)player, "waystone", "Waystone Discovered");
            }
            else
            {
                // Otherwise, send open packet.
                List<WaystoneData> waystoneList = new();

                foreach (WaystoneData waystoneData in waystones.Values)
                {
                    if (waystoneData.discoveredByPlayerUids.Contains(player.PlayerUID))
                    {
                        waystoneList.Add(waystoneData);
                    }
                }

                WaystoneGuiPayload payload = new(waystoneList, new GridPos(glyph.obelisk.Pos));

                SendPacket(payload, (IServerPlayer)player);
            }

        }
    }

    public override void OnStart()
    {
        if (isServer)
        {
            MainAPI.Sapi.Event.GameWorldSave += SaveData;
            LoadData();
        }
        else
        {
            MareShaderRegistry.AddShader("obelisks:cogs", "obelisks:cogs", "cogs");
        }
    }

    public void LoadData()
    {
        byte[]? data = MainAPI.Sapi.WorldManager.SaveGame.GetData("waystones");
        if (data == null) return;

        try
        {
            Dictionary<GridPos, WaystoneData> loadedData = SerializerUtil.Deserialize<Dictionary<GridPos, WaystoneData>>(data);
            waystones = loadedData;
        }
        catch
        {
            Console.WriteLine("Error loading waystone data.");
        }
    }

    private void SaveData()
    {
        MainAPI.Sapi.WorldManager.SaveGame.StoreData("waystones", SerializerUtil.Serialize(waystones));
    }
}