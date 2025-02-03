using MareLib;
using OpenTK.Mathematics;
using System;

namespace Obelisks;

public class TexturedButton : BaseButtonWidget
{
    public Texture button;

    public TexturedButton(Gui gui, Bounds bounds, Action onClick, Texture nonDisposed) : base(gui, bounds, onClick)
    {
        button = nonDisposed;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        shader.BindTexture(button, "tex2d");

        if (state == ButtonState.Active)
        {
            shader.Uniform("color", new Vector4(0.8f, 0.8f, 0.8f, 1));
            RenderTools.RenderQuad(shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            shader.Uniform("color", Vector4.One);
        }

        if (state == ButtonState.Hovered)
        {
            shader.Uniform("color", new Vector4(1.1f, 1.1f, 0.9f, 1));
            RenderTools.RenderQuad(shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            shader.Uniform("color", Vector4.One);
        }

        if (state == ButtonState.Normal)
        {
            RenderTools.RenderQuad(shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }
    }
}