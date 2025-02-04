using MareLib;
using System;
using System.Collections.Generic;

namespace Obelisks;

public static class ObeliskGuiThemes
{
    private static readonly Dictionary<string, object> cache = new();

    public static NineSliceTexture Inside => GetOrCreate("rocktexturein", () => RockRectangle(64, 64, 8, true));
    public static NineSliceTexture Outside => GetOrCreate("rocktextureout", () => RockRectangle(64, 64, 8, false));
    public static Texture IconBg => GetOrCreate("iconbg", () => Texture.Create("obelisks:textures/iconbg.png"));

    public static Texture Obelisk => GetOrCreate("obelisktex", () => Texture.Create("obelisks:textures/obelisk.png"));

    private static NineSliceTexture RockRectangle(int width, int height, int stroke, bool inside)
    {
        // Gradient example.

        //TextureBuilder builder = TextureBuilder.Begin(width, height);

        //SKColor green = SkiaThemes.Uncommon;
        //SKColor darkerGreen = new((byte)(green.Red - 30), (byte)(green.Green - 30), (byte)(green.Blue - 30));

        //SKColor[] colors = new SKColor[] { green, darkerGreen };
        //float[] positions = new float[] { 0.0f, 1.0f };

        //SKShader gradient = SKShader.CreateLinearGradient(
        //new SKPoint(0, 0),  // Start point (top left corner).
        //new SKPoint(100, 100), // End point (bottom right corner).
        //colors, positions, SKShaderTileMode.Clamp);

        //builder.Shader(gradient);

        //return builder.FillMode()
        //    .DrawRectangle(0, 0, width, height)
        //    .StrokeMode(4)
        //    .DrawEmbossedRectangle(0, 0, width, height, inside)
        //    .End()
        //    .AsNineSlice(8, 8);

        return Texture.Create("obelisks:textures/runes/panel.png").AsNineSlice(14, 14);
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
}