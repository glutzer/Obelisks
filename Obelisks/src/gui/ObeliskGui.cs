using MareLib;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace Obelisks;

public static class ObeliskGui
{
    private static readonly Dictionary<string, object> cache = new();

    public static NineSliceTexture RockInside => GetOrCreate("rocktexturein", () => RockRectangle(512, 512, 8, true));
    public static NineSliceTexture RockOutside => GetOrCreate("rocktextureout", () => RockRectangle(512, 512, 8, false));

    public static Texture ButtonUp => GetOrCreate("buttonup", () => Texture.Create("obelisks:textures/buttonup.png"));
    public static Texture ButtonDown => GetOrCreate("buttondown", () => Texture.Create("obelisks:textures/buttondown.png"));
    public static Texture X => GetOrCreate("x", () => Texture.Create("obelisks:textures/x.png"));
    public static Texture IconBg => GetOrCreate("buttondown", () => Texture.Create("obelisks:textures/iconbg.png"));

    private static NineSliceTexture RockRectangle(int width, int height, int stroke, bool inside)
    {
        TextureBuilder builder = TextureBuilder.Begin(width, height);

        SKBitmap cachedBitmap = GetOrCreate("skiabitmap", () => GetBitmap("obelisks:textures/jagged.png"));

        SKShader shader = SKShader
            .CreateBitmap(cachedBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat)
            .WithLocalMatrix(SKMatrix.CreateScale(1, 1));

        SKColor obsidianPurple = new(0x4B, 0x2C, 0x6E);

        SKColorFilter filter = SKColorFilter.CreateBlendMode(obsidianPurple, SKBlendMode.Multiply);
        shader = shader.WithColorFilter(filter);

        builder.Shader(shader);

        builder.FillMode();
        return builder.DrawRectangle(0, 0, width, height)
            .StrokeMode(stroke)
            .DrawEmbossedRectangle(0, 0, width, height, inside)
            .End()
            .AsNineSlice(stroke, stroke);
    }

    private static T GetOrCreate<T>(string path, Func<T> makeTex)
    {
        if (cache.TryGetValue(path, out object? value))
        {
            return (T)value;
        }
        else
        {
            object tex = makeTex()!;
            cache.Add(path, tex);
            return (T)tex;
        }
    }

    public static void ClearCache()
    {
        foreach (object obj in cache)
        {
            if (obj is IDisposable tex)
            {
                tex.Dispose();
            }
        }

        cache.Clear();
    }

    private static SKBitmap GetBitmap(string assetPath)
    {
        IAsset? textureAsset = MainAPI.Capi.Assets.Get(new AssetLocation(assetPath)) ?? throw new Exception($"Texture asset not found: {assetPath}!");
        byte[] pngData = textureAsset.Data;
        SKBitmap bmp = SKBitmap.Decode(pngData);
        return bmp;
    }
}