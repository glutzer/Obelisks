using MareLib;
using OpenTK.Mathematics;
using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Obelisks;

public enum ObeliskSelection
{
    Obelisk,
    North,
    East,
    South,
    West
}

[BlockEntity]
public class BlockEntityObelisk : BlockEntity, IRenderer
{
    public MeshHandle? obeliskMesh;
    public MeshHandle? runeMesh;
    public Texture rune1 = null!;
    public Texture rune2 = null!;
    public TextureAtlasPosition? obeliskTexPos;

    public Inscription? currentGlyph;
    public Inscription[] runes = new Inscription[4];

    public ObeliskStats stats = new(0);

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        if (api is ICoreClientAPI)
        {
            rune1 = ClientCache.GetOrCache("runeTexture1", () => Texture.Create("obelisks:textures/runes/rune1.png"));
            rune2 = ClientCache.GetOrCache("runeTexture2", () => Texture.Create("obelisks:textures/runes/rune2.png"));
            InitializeRenderer();
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);

        if (currentGlyph != null)
        {
            ITreeAttribute glyph = tree.GetOrAddTreeAttribute("currentGlyph");
            glyph.SetString("code", currentGlyph.code);
            currentGlyph.ToTreeAttributes(glyph);
        }

        for (int i = 0; i < 4; i++)
        {
            if (runes[i] != null)
            {
                ITreeAttribute rune = tree.GetOrAddTreeAttribute($"rune{i}");
                rune.SetString("code", runes[i].code);
                runes[i].ToTreeAttributes(rune);
            }
        }

        tree.SetInt("potentia", stats.Potentia);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);

        worldAccessForResolve.Api.Event.EnqueueMainThreadTask(() =>
        {
            ObelisksSystem obeliskSystem = MainAPI.GetGameSystem<ObelisksSystem>(worldAccessForResolve.Side);

            stats.Potentia = tree.GetInt("potentia", 0);

            if (tree.HasAttribute("currentGlyph"))
            {
                ITreeAttribute glyphData = tree.GetTreeAttribute("currentGlyph");
                string? glyphCode = glyphData.GetString("code");

                if (glyphCode != null)
                {
                    obeliskSystem.glyphTypes.TryGetValue(glyphCode, out Type? type);
                    if (type != null)
                    {
                        if (currentGlyph == null || currentGlyph.code != glyphCode)
                        {
                            currentGlyph?.OnRemoved();
                            currentGlyph = (Inscription)Activator.CreateInstance(type, this, Pos, worldAccessForResolve)!;
                            currentGlyph.OnAdded();
                        }

                        currentGlyph.FromTreeAttributes(glyphData);
                    }
                }
            }
            else
            {
                currentGlyph?.OnRemoved();
                currentGlyph = null;
            }

            for (int i = 0; i < 4; i++)
            {
                if (tree.HasAttribute($"rune{i}"))
                {
                    ITreeAttribute runeData = tree.GetTreeAttribute($"rune{i}");
                    string? runeCode = runeData.GetString("code");
                    if (runeCode != null)
                    {
                        obeliskSystem.runeTypes.TryGetValue(runeCode, out Type? type);
                        if (type != null)
                        {
                            if (runes[i] == null || runes[i].code != runeCode)
                            {
                                runes[i]?.OnRemoved();
                                runes[i] = (Inscription)Activator.CreateInstance(type, this, Pos, worldAccessForResolve)!;
                                runes[i].OnAdded();
                            }

                            runes[i].FromTreeAttributes(runeData);
                        }
                    }
                }
                else
                {
                    runes[i]?.OnRemoved();
                    runes[i] = null!;
                }
            }

            UpdateStats();
        }, "loadObelisk");
    }

    /// <summary>
    /// Handle server-side interaction.
    /// </summary>
    public void HandleCtrlInteraction(IServerPlayer player, ObeliskSelection selection)
    {
        ItemStack? handStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;
        if (handStack == null) return;

        if (selection == ObeliskSelection.Obelisk)
        {
            if (MainAPI.GetGameSystem<ObelisksSystem>(Api.Side).TryGetGlyphType(handStack, out Type? type))
            {
                if (type == currentGlyph?.GetType())
                {
                    currentGlyph.AddPower(handStack);
                    MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/rune", Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                }
                else
                {
                    currentGlyph?.OnRemoved();
                    currentGlyph?.OnDestroyed();
                    currentGlyph = (Inscription)Activator.CreateInstance(type, this, Pos, Api.World)!;
                    currentGlyph.OnAdded();
                    currentGlyph.OnCreated();
                    currentGlyph.AddPower(handStack);
                    MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/convert", Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                }
                MarkDirty(true);
            }
        }
        else
        {
            int index = (int)selection - 1;
            if (MainAPI.GetGameSystem<ObelisksSystem>(Api.Side).TryGetRuneType(handStack, out Type? type))
            {
                if (type == runes[index]?.GetType())
                {
                    runes[index].AddPower(handStack);
                    MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/rune", Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                }
                else
                {
                    runes[index]?.OnRemoved();
                    runes[index]?.OnDestroyed();
                    runes[index] = (Inscription)Activator.CreateInstance(type, this, Pos, Api.World)!;
                    runes[index].OnAdded();
                    runes[index].OnCreated();
                    runes[index].AddPower(handStack);
                    MainAPI.Sapi.World.PlaySoundAt("obelisks:sounds/convert", Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                }
                MarkDirty(true);
            }
        }

        if (player.InventoryManager.ActiveHotbarSlot.Itemstack?.StackSize <= 0) player.InventoryManager.ActiveHotbarSlot.TakeOutWhole();
        player.InventoryManager.ActiveHotbarSlot.MarkDirty();
        UpdateStats();
    }

    // ########## Rendering.

    public void RemakeRuneMesh()
    {
        runeMesh?.Dispose();

        runeMesh = QuadMeshUtility.CreateCenteredQuadMesh(vert =>
        {
            vert.position.X *= 0.7f;

            if (vert.position.Y > 0)
            {
                vert.position.Y += 0.5f;
                vert.position.Z += 0.05f;
            }

            vert.position.Z += 0.1f;

            vert.position.Y += 0.5f;

            return new StandardVertex(vert.position + new Vector3(0.5f, 0.5f, 0), vert.uv, -vert.normal, Vector4.One);
        });
    }

    public void InitializeRenderer()
    {
        RemakeRuneMesh();

        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.OIT);

        IAsset shapeAsset = MainAPI.Capi.Assets.TryGet("obelisks:shapes/obelisk.json");
        Shape obeliskShape = shapeAsset.ToObject<Shape>();

        obeliskShape.Elements = obeliskShape.Elements.Where(e => e.Name.StartsWith("Cube")).ToArray();

        ShapeTextureSource textureSource = new(MainAPI.Capi, obeliskShape, "");
        MainAPI.Capi.Tesselator.TesselateShape("", obeliskShape, out MeshData meshData, textureSource);

        MeshInfo<StandardVertex> standardMesh = new(6, 6);

        Vector2 minUv = new(float.MaxValue);
        Vector2 maxUv = new(float.MinValue);

        for (int i = 0; i < meshData.VerticesCount; i++)
        {
            float y = meshData.xyz[(i * 3) + 1];

            float x = meshData.xyz[i * 3];
            float z = meshData.xyz[(i * 3) + 2];

            float multi;

            if (y <= 2)
            {
                multi = 1 - (y / 8);

                if (y > 0) y += 0.5f;
            }
            else
            {
                multi = 1 - (y / 3);
            }

            y -= 0.05f;

            x -= 0.5f;
            z -= 0.5f;

            x *= multi;
            z *= multi;

            x += 0.5f;
            z += 0.5f;

            StandardVertex vertex = new(
                new Vector3(x, y, z),
                new Vector2(meshData.Uv[i * 2], meshData.Uv[(i * 2) + 1]),
                Vector3.One,
                Vector4.One);

            standardMesh.AddVertex(vertex);

            float xUv = meshData.Uv[i * 2];
            float yUv = meshData.Uv[(i * 2) + 1];

            minUv.X = Math.Min(xUv, minUv.X);
            minUv.Y = Math.Min(yUv, minUv.Y);

            maxUv.X = Math.Max(xUv, maxUv.X);
            maxUv.Y = Math.Max(yUv, maxUv.Y);
        }

        for (int i = 0; i < meshData.IndicesCount; i++)
        {
            standardMesh.AddIndex(meshData.Indices[i]);
        }

        obeliskMesh = standardMesh.Upload();

        obeliskTexPos = new() { x1 = minUv.X, y1 = minUv.Y, x2 = maxUv.X, y2 = maxUv.Y };
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        OnBlockGone();
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        OnBlockGone();
        currentGlyph?.OnDestroyed();
        foreach (Inscription? item in runes)
        {
            item?.OnDestroyed();
        }
    }

    public void OnBlockGone()
    {
        currentGlyph?.OnRemoved();
        foreach (Inscription? item in runes)
        {
            item?.OnRemoved();
        }

        if (Api.Side != EnumAppSide.Client) return;
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.OIT);
        Dispose();
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        return true;
    }

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        if (obeliskMesh == null || runeMesh == null || rune1 == null || obeliskTexPos == null) return;

        IShaderProgram current = MainAPI.Capi.Render.CurrentActiveShader;

        float leviatingY = (MathF.Sin(MainAPI.Capi.ElapsedMilliseconds / 1000f) + 2f) * stats.PotentiaPercent * 0.1f;

        if (stage == EnumRenderStage.Opaque)
        {
            MareShader shader = MareShaderRegistry.Get("obeliskopaque");
            Matrix4 mat = RenderTools.CameraRelativeTranslation(Pos.X, Pos.Y + leviatingY, Pos.Z);
            shader.Use();
            shader.Uniform("modelMatrix", mat);
            // Hope this is the right id lol...
            shader.BindTexture(MainAPI.Capi.BlockTextureAtlas.AtlasTextures[0].TextureId, "tex2d");
            int ACTUAL_VALUE = MainAPI.Client.blockAccessor.GetLightRGBsAsInt(Pos.X, Pos.Y, Pos.Z);
            Vector4 light = new((ACTUAL_VALUE & 0xFF) / 255f, ((ACTUAL_VALUE >> 8) & 0xFF) / 255f, ((ACTUAL_VALUE >> 16) & 0xFF) / 255f, ((ACTUAL_VALUE >> 24) & 0xFF) / 255f);
            shader.Uniform("blockLightIn", light);
            shader.LightUniforms();
            shader.ShadowUniforms();
            shader.Uniform("time", MainAPI.Capi.ElapsedMilliseconds / 1000f);
            RenderTools.RenderMesh(obeliskMesh);
        }

        if (currentGlyph == null && runes.All(r => r == null)) return;

        if (stage == EnumRenderStage.OIT)
        {
            MareShader shader = MareShaderRegistry.Get("runeoit");
            Matrix4 mat = RenderTools.CameraRelativeTranslation(Pos.X, Pos.Y + leviatingY, Pos.Z);
            shader.Use();
            shader.Uniform("modelMatrix", mat);
            // Hope this is the right id lol...
            shader.BindTexture(MainAPI.Capi.BlockTextureAtlas.AtlasTextures[0].TextureId, "tex2d");
            int ACTUAL_VALUE = MainAPI.Client.blockAccessor.GetLightRGBsAsInt(Pos.X, Pos.Y, Pos.Z);
            Vector4 light = new((ACTUAL_VALUE & 0xFF) / 255f, ((ACTUAL_VALUE >> 8) & 0xFF) / 255f, ((ACTUAL_VALUE >> 16) & 0xFF) / 255f, ((ACTUAL_VALUE >> 24) & 0xFF) / 255f);
            shader.Uniform("blockLightIn", light);
            shader.LightUniforms();
            shader.ShadowUniforms();
            shader.Uniform("time", MainAPI.Capi.ElapsedMilliseconds / 1000f);

            Matrix4 matObelisk = Matrix4.CreateTranslation(-0.5f, -0.3f, -0.5f) * Matrix4.CreateScale(1.1f, 1.1f, 1.1f) * Matrix4.CreateTranslation(0.5f, 0.3f, 0.5f) * mat;

            shader.BindTexture(rune2, "tex2d");
            shader.Uniform("atlasMap", new Vector4(obeliskTexPos.x1, obeliskTexPos.y1, obeliskTexPos.x2, obeliskTexPos.y2));

            if (currentGlyph != null)
            {
                Vector4 color = currentGlyph.Color;
                color.W *= currentGlyph.PowerPercent;

                shader.Uniform("color", color);

                shader.Uniform("modelMatrix", matObelisk);

                RenderTools.RenderMesh(obeliskMesh);
            }

            shader.BindTexture(rune1, "tex2d");
            shader.Uniform("atlasMap", new Vector4(0, 0, 1, 1));

            for (int i = 0; i < 4; i++)
            {
                if (runes[i] == null) continue;

                Vector4 color = runes[i].Color;
                color.W *= runes[i].PowerPercent;

                shader.Uniform("color", color);

                shader.Uniform("modelMatrix", Matrix4.CreateTranslation(-0.5f, -0.3f, -0.5f) * Matrix4.CreateRotationY(i * MathHelper.DegreesToRadians(-90)) * Matrix4.CreateTranslation(0.5f, 0.3f, 0.5f) * mat);

                RenderTools.RenderMesh(runeMesh);
            }
        }

        current?.Use();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        obeliskMesh?.Dispose();
    }

    public double RenderOrder => 0.5;
    public int RenderRange => 0;

    public override void GetBlockInfo(IPlayer forPlayer, System.Text.StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);

        dsc.AppendLine($"Potentia: {stats.Potentia}/{stats.maxPotentia}");

        currentGlyph?.GetInfo(dsc);

        for (int i = 0; i < 4; i++)
        {
            runes[i]?.GetInfo(dsc);
        }
    }

    public void UpdateStats()
    {
        ObeliskStats newStats = new(stats.Potentia);

        for (int i = 0; i < 4; i++)
        {
            runes[i]?.ModifyStats(newStats);
        }

        currentGlyph?.ModifyStats(newStats);

        stats = newStats;
    }
}