using MareLib;
using Obelisks.src.glyphs.bases;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Obelisks;

[Glyph("game:gem-olivine_peridot-rough")]
public class GlyphOfStabilization : FieldGlyph, IPhysicsTickable
{
    public override float FieldRange => PowerPercent * 10 * obelisk.stats.aoeMultiplier;
    private long longIntervalId;
    public override Vector4 Color => new(0, 0.3f, 1, 1);
    private readonly List<Entity> enemies = new();
    private readonly List<EntityPlayer> players = new();

    public GlyphOfStabilization(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world, "obelisks:objs/icosphere.obj")
    {

    }

    public override void OnAdded()
    {
        base.OnAdded();

        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntitySpawn += Event_OnEntitySpawn;
            longIntervalId = MainAPI.Sapi.Event.RegisterGameTickListener(LongIntervalCheck, 5000);
            MainAPI.Sapi.Server.AddPhysicsTickable(this);
        }
    }

    public override void OnRemoved()
    {
        base.OnRemoved();

        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntitySpawn -= Event_OnEntitySpawn;
            MainAPI.Sapi.Event.UnregisterGameTickListener(longIntervalId);
            MainAPI.Sapi.Server.RemovePhysicsTickable(this);
        }
    }

    private void LongIntervalCheck(float dt)
    {
        enemies.Clear();
        players.Clear();

        Entity[] entities = MainAPI.Server.GetEntitiesAround(obelisk.Pos.ToVec3d().Add(0.5, 0, 0.5), FieldRange + 10, FieldRange + 10, entity =>
        {
            double dist = entity.Pos.DistanceTo(obelisk.Pos.ToVec3d()) - 10;
            return dist < FieldRange;
        });

        // 5_000 seconds by default to drain.
        if (MainAPI.Sapi.World.Rand.Next(10) == 1)
        {
            int potentiaDrain = (int)(10 * obelisk.stats.powerMultiplier);
            obelisk.stats.AddPotentia(-potentiaDrain);
        }

        foreach (Entity item in entities)
        {
            if (item is EntityPlayer player) players.Add(player);
            if (item.Properties.Attributes?.KeyExists("spawnCloserDuringLowStability") ?? false) enemies.Add(item);
        }

        // Add stability to every player.
        foreach (EntityPlayer player in players)
        {
            if (player.GetBehavior<EntityBehaviorTemporalStabilityAffected>() is EntityBehaviorTemporalStabilityAffected stabilityAffected)
            {
                stabilityAffected.CallMethod("AddStability", 0.01f * obelisk.stats.powerMultiplier);
            }
        }
    }

    private void Event_OnEntitySpawn(Entity entity)
    {
        double dist = entity.Pos.DistanceTo(obelisk.Pos.ToVec3d());

        if (dist < FieldRange && obelisk.stats.Potentia > 100)
        {
            if (entity.Attributes == null) return;
            if (entity.Properties.Attributes.KeyExists("spawnCloserDuringLowStability"))
            {
                entity.Die(EnumDespawnReason.Expire);
            }
        }
    }

    public bool Ticking { get; set; } = true;
    bool IPhysicsTickable.Ticking { get => Ticking; set => Ticking = value; }

    public void OnPhysicsTick(float dt)
    {
        Vector3d obeliskPos = new(obelisk.Pos.X + 0.5, obelisk.Pos.Y, obelisk.Pos.Z + 0.5);

        // Interdiction torch.
        foreach (Entity entity in enemies)
        {
            double dist = entity.Pos.DistanceTo(obelisk.Pos.ToVec3d());
            if (dist > FieldRange) continue;

            Vector3d entityPos = new(entity.Pos.X, entity.Pos.Y, entity.Pos.Z);

            Vector3d normal = (entityPos - obeliskPos).Normalized();

            entity.ServerPos.Motion.Add(normal.X * 0.1f, normal.Y * 0.02f, normal.Z * 0.1f);
        }
    }

    public void AfterPhysicsTick(float dt)
    {

    }

    public bool CanProceedOnThisThread()
    {
        return true;
    }

    public void OnPhysicsTickDone()
    {

    }
}