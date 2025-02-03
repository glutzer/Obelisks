using MareLib;
using OpenTK.Mathematics;
using System;

namespace Obelisks;

public class RockButton : BaseButtonWidget
{
    public NineSliceTexture inside;
    public NineSliceTexture outside;
    public TextObject textObj;

    public RockButton(Gui gui, Bounds bounds, Action onClick, string text, int fontScale) : base(gui, bounds, onClick)
    {
        inside = ObeliskGui.RockInside;
        outside = ObeliskGui.RockOutside;
        textObj = new TextObject(text, FontRegistry.GetFont("friz"), fontScale, new Vector4(0.9f, 0.2f, 0.6f, 0.7f));
        textObj.Italic(true);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (state == ButtonState.Active)
        {
            shader.Uniform("color", new Vector4(0.8f, 0.8f, 0.8f, 1));
            RenderTools.RenderNineSlice(inside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            shader.Uniform("color", Vector4.One);
        }

        if (state == ButtonState.Hovered)
        {
            shader.Uniform("color", new Vector4(1.1f, 1.1f, 0.9f, 1));
            RenderTools.RenderNineSlice(outside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            shader.Uniform("color", Vector4.One);
        }

        if (state == ButtonState.Normal)
        {
            RenderTools.RenderNineSlice(outside, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        textObj.RenderCenteredLine(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), shader, true);
    }
}