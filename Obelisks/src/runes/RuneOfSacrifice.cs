using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Obelisks;

[Rune("game:ore-cinnabar")]
public class RuneOfSacrifice : Inscription
{
    public override int MaxPower => 50;
    public override Vector4 Color => new(1, 0, 0, 1);

    private static readonly SimpleParticleProperties liquidParticles;
    static RuneOfSacrifice()
    {
        liquidParticles = new SimpleParticleProperties()
        {
            MinVelocity = new Vec3f(-6f, -6f, -6f),
            AddVelocity = new Vec3f(12f, 12f, 12f),
            addLifeLength = 4f,
            LifeLength = 0.5f,
            MinQuantity = 40,
            AddQuantity = 20,
            GravityEffect = 2f,
            SelfPropelled = false,
            MinSize = 0.5f,
            MaxSize = 2f,
            Color = ColorUtil.ToRgba(100, 200, 0, 0)
        };
    }

    public RuneOfSacrifice(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world) : base(obelisk, pos, world)
    {
    }

    public override void OnAdded()
    {
        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntityDeath += Event_OnEntityDeath;
        }
    }

    private void Event_OnEntityDeath(Entity entity, DamageSource? damageSource)
    {
        if (damageSource != null && damageSource.Source != EnumDamageSource.Unknown && entity.Pos.DistanceTo(obelisk.Pos.ToVec3d()) < currentPower / 5 * obelisk.stats.aoeMultiplier)
        {
            if (isServer && !entity.Attributes.GetBool("sacrificed"))
            {
                // Add 20x the entity's max HP to the obelisk.
                if (entity.WatchedAttributes.GetTreeAttribute("health") is TreeAttribute health)
                {
                    float maxHealth = health.GetFloat("maxhealth");
                    obelisk.stats.AddPotentia((int)maxHealth * 20);
                }

                obelisk.MarkDirty();

                //entity.Die(EnumDespawnReason.Expire);
                entity.Attributes.SetBool("sacrificed", true);

                liquidParticles.MinPos = entity.Pos.XYZ;
                liquidParticles.AddPos = new Vec3d(0, 0, 0);

                Vec3d toObelisk = obelisk.Pos.ToVec3d().Add(0.5f, 3f, 0.5f) - entity.Pos.XYZ;

                liquidParticles.MinVelocity.Set(toObelisk * 2);
                liquidParticles.AddVelocity.Set(toObelisk);

                MainAPI.Sapi.World.SpawnParticles(liquidParticles);
                MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/corpseexplode", entity);
            }
        }
    }

    public override void OnRemoved()
    {
        if (isServer)
        {
            MainAPI.Sapi.Event.OnEntityDeath -= Event_OnEntityDeath;
        }
    }
}