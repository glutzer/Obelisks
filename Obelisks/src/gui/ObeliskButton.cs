using MareLib;
using OpenTK.Mathematics;
using System;

namespace Obelisks;

public class ObeliskButton : BaseButtonWidget
{
    public NineSliceTexture inside;
    public NineSliceTexture outside;
    public TextObject textObj;
    public TextObjectIndecipherable textObjIndecipherable;

    private Accumulator accum;
    private float timeToUseCipher;

    public ObeliskButton(Gui gui, Bounds bounds, Action onClick, string text, int fontScale) : base(gui, bounds, onClick)
    {
        inside = ObeliskGuiThemes.Inside;
        outside = ObeliskGuiThemes.Outside;

        accum.SetInterval(0.1f);
        accum.Max(1);

        textObj = new TextObject(text, FontRegistry.GetFont("friz"), fontScale, new Vector4(0, 0.2f, 0, 1f));
        textObjIndecipherable = new TextObjectIndecipherable(text, FontRegistry.GetFont("runic"), fontScale, new Vector4(0, 0.2f, 0, 1f), FontRegistry.GetFont("friz"), CipherType.AllRandomized);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        accum.Add(dt);
        while (accum.Consume())
        {
            if (MainAPI.Capi.World.Rand.Next(30) == 0)
            {
                timeToUseCipher += 0.5f;
            }
        }

        if (state == ButtonState.Active)
        {
            shader.Uniform("color", new Vector4(0.3f, 0.8f, 0.3f, 0.5f));
            RenderTools.RenderNineSlice(inside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        if (state == ButtonState.Hovered)
        {
            shader.Uniform("color", new Vector4(0.6f, 1.1f, 0.6f, 0.5f));
            RenderTools.RenderNineSlice(outside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        if (state == ButtonState.Normal)
        {
            shader.Uniform("color", new Vector4(0.5f, 1f, 0.5f, 0.5f));
            RenderTools.RenderNineSlice(outside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        shader.Uniform("color", Vector4.One);

        if (timeToUseCipher > 0)
        {
            textObjIndecipherable.RenderCenteredLine(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), shader, true);
            timeToUseCipher -= dt;
        }
        else
        {
            textObj.RenderCenteredLine(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), shader, true);
        }
    }
}