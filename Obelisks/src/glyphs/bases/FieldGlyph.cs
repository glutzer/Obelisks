using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Obelisks.src.glyphs.bases;

public abstract class FieldGlyph : Inscription
{
    public abstract float FieldRange { get; }
    private long distortionInstance;
    private readonly MeshHandle icoSphere = null!;

    public FieldGlyph(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world, string fieldShape) : base(obelisk, pos, world)
    {
        icoSphere = ObjLoader.LoadObj(fieldShape, vert =>
        {
            return new StandardVertex(vert.position, vert.uv, vert.normal, Vector4.One);
        }).Upload();
    }

    public override void OnAdded()
    {
        if (!isServer)
        {
            distortionInstance = MainAPI.GetGameSystem<DistortionSystem>(EnumAppSide.Client).RegisterRenderer(RenderDistortion);
        }
    }

    public override void OnRemoved()
    {
        if (!isServer)
        {
            MainAPI.GetGameSystem<DistortionSystem>(EnumAppSide.Client).UnregisterRenderer(distortionInstance);
            icoSphere.Dispose();
        }
    }

    public virtual void RenderDistortion(float dt, MareShader shader)
    {
        float seconds = TimeUtility.ElapsedClientSeconds() * 0.1f;

        Quaternion rotation = Quaternion.FromEulerAngles(seconds, seconds, seconds);

        float fieldRange = FieldRange;
        Matrix4 mat = RenderTools.CameraRelativeTranslation(obelisk.Pos.X + 0.5, obelisk.Pos.Y, obelisk.Pos.Z + 0.5);
        mat = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateScale(fieldRange, fieldRange, fieldRange) * mat;

        // Full effect visible at 100 potentia.
        shader.Uniform("strength", Math.Clamp(obelisk.stats.Potentia / 100f, 0, 1));

        shader.Uniform("modelMatrix", mat);
        shader.Uniform("useFresnel", 1);
        shader.Uniform("fresnelColor", Color * Math.Clamp(obelisk.stats.Potentia / 100f, 0, 1));

        RenderTools.RenderMesh(icoSphere);

        shader.Uniform("useFresnel", 0);
        shader.Uniform("strength", 1f);
    }
}