using MareLib;
using Obelisks.src.glyphs.bases;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Obelisks;

[Glyph("game:gem-diamond-rough")]
public class GlyphOfReflection : FieldGlyph, IPhysicsTickable
{
    public override float FieldRange => PowerPercent * 10 * obelisk.stats.aoeMultiplier;
    public override Vector4 Color => new(1, 1, 1, 1);
    private readonly Queue<Entity> trackedProjectiles = new();

    public GlyphOfReflection(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world, "obelisks:objs/sphere.obj")
    {

    }

    public override void OnAdded()
    {
        base.OnAdded();

        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntitySpawn += Event_OnEntitySpawn;
            MainAPI.Sapi.Server.AddPhysicsTickable(this);
        }
    }

    public override void OnRemoved()
    {
        base.OnRemoved();

        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntitySpawn -= Event_OnEntitySpawn;
            MainAPI.Sapi.Server.RemovePhysicsTickable(this);
        }
    }

    private void Event_OnEntitySpawn(Entity entity)
    {
        if (entity is not EntityProjectile and not EntityThrownStone) return;

        double dist = entity.Pos.DistanceTo(obelisk.Pos.ToVec3d());

        if (dist < FieldRange + 300 && obelisk.stats.Potentia > 100)
        {
            trackedProjectiles.Enqueue(entity);
        }
    }

    public bool Ticking { get; set; } = true;
    bool IPhysicsTickable.Ticking { get => Ticking; set => Ticking = value; }

    public void OnPhysicsTick(float dt)
    {
        Vector3d obeliskPos = new(obelisk.Pos.X + 0.5, obelisk.Pos.Y, obelisk.Pos.Z + 0.5);

        int count = trackedProjectiles.Count;

        float fieldRange = FieldRange;

        for (int i = 0; i < count; i++)
        {
            Entity proj = trackedProjectiles.Dequeue();
            Vector3d position = new(proj.ServerPos.X, proj.ServerPos.Y, proj.ServerPos.Z);

            Vector3d normalOut = (position - obeliskPos).Normalized();

            Vector3d motion = new(proj.ServerPos.Motion.X, proj.ServerPos.Motion.Y, proj.ServerPos.Motion.Z);
            double motionLength = motion.Length;
            motion.Normalize();

            // Projectile is going into the bubble.
            if (Vector3d.Dot(motion, normalOut) < 0 && Vector3d.Distance(position, obeliskPos) < fieldRange)
            {
                proj.ServerPos.Motion.Set(normalOut.X * motionLength * 0.25, normalOut.Y * motionLength * 0.25, normalOut.Z * motionLength * 0.25);
                MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/convert", position.X, position.Y, position.Z);

                obelisk.stats.AddPotentia((int)(-100 * obelisk.stats.powerMultiplier));
            }
            else if (motionLength > 0.005)
            {
                trackedProjectiles.Enqueue(proj); // Projectile basically stopped.
            }
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