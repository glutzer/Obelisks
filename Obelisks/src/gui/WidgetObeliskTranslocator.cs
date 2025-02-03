using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Obelisks;

public class WidgetObeliskTranslocator : Widget
{
    private Vector2 mouseOffset;
    private readonly Vector2d initialPos;

    private float blocksPerPixel = 1;

    private bool dragging;
    private readonly List<WaystoneData> waystoneData;
    private Vector2d currentStonePos;

    private Texture ObeliskTex { get; } = ObeliskGui.ButtonUp;
    private Texture IconBg { get; } = ObeliskGui.IconBg;

    public WaystoneData? currentMousedWaystone;
    public WaystoneData? currentSelectedWaystone;
    public float timeMoused;

    public TextObject normalFont;
    public TextObjectIndecipherable cipherFont;
    public Accumulator accumulator;

    private Bounds? buttonBounds;

    public WidgetObeliskTranslocator(Gui gui, Bounds bounds, Vector2d centerPos, List<WaystoneData> waystoneData, GridPos currentStonePos) : base(gui, bounds)
    {
        initialPos = centerPos;
        this.waystoneData = waystoneData;
        this.currentStonePos = new Vector2d(currentStonePos.X, currentStonePos.Z);

        normalFont = new TextObject("", FontRegistry.GetFont("friz"), 30, new Vector4(0, 1, 0.3f, 1));
        cipherFont = new TextObjectIndecipherable("", FontRegistry.GetFont("runic"), 30, Vector4.One, FontRegistry.GetFont("friz"), CipherType.FirstRandomized);
        accumulator.SetInterval(0.05f);
    }

    public void AddTeleportButton()
    {
        if (currentSelectedWaystone == null) return;

        children?.Clear();
        bounds.children?.Clear();

        if (new Vector2d(currentSelectedWaystone.position.X, currentSelectedWaystone.position.Z) == currentStonePos) return;

        buttonBounds = Bounds.CreateFrom(bounds).Alignment(Align.LeftTop).Fixed(0, 0, 256, 64);
        AddChild(new RockButton(gui, buttonBounds, () =>
        {
            ObeliskTeleportGui obGui = (ObeliskTeleportGui)gui;
            if (obGui.obelisk.stats.Potentia < 100)
            {
                MainAPI.Capi.TriggerIngameError(this, "notenoughpotentia", "100 potentia required for ascension.");
                return;
            }
            MainAPI.GetGameSystem<TranslocationSystem>(EnumAppSide.Client).SendPacket(new WaystoneRequestPacket(currentSelectedWaystone.position, new GridPos(obGui.obelisk.Pos), WaystoneRequestType.DoTranslocation));
            gui.TryClose();
        }, "Ascend From Flesh", 30));

        gui.MarkForRepartition();
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseWheel += GuiEvents_MouseWheel;
    }

    private void GuiEvents_MouseWheel(MouseWheelEventArgs obj)
    {
        obj.SetHandled();

        float oldPixel = blocksPerPixel;

        if (obj.delta < 0)
        {
            blocksPerPixel = Math.Clamp(blocksPerPixel * 2, 1, 32);
            if (oldPixel != blocksPerPixel) mouseOffset /= 2;
        }
        else
        {
            blocksPerPixel = Math.Clamp(blocksPerPixel / 2, 1, 32);
            if (oldPixel != blocksPerPixel) mouseOffset *= 2;
        }
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (dragging)
        {
            mouseOffset += new Vector2(-obj.DeltaX, -obj.DeltaY);
            obj.Handled = true;
        }

        if (bounds.IsInAllBounds(obj))
        {
            foreach (WaystoneData data in waystoneData)
            {
                if (data == currentSelectedWaystone) continue;

                Vector2d pos = new(data.position.X, data.position.Z);
                Vector2i pixel = GetPixelOfPosition(pos);

                if (Bounds.IsInAllBounds(obj.X, obj.Y, pixel.X - 32, pixel.Y - 32, 64, 64))
                {
                    if (data != currentMousedWaystone)
                    {
                        timeMoused = 0;

                        if (new Vector2d(data.position.X, data.position.Z) == currentStonePos)
                        {
                            cipherFont.Text = "Current Location";
                        }
                        else
                        {
                            Vintagestory.API.Common.Entities.EntityPos spawn = MainAPI.Capi.World.DefaultSpawnPosition;

                            Vector3d distance = new(data.position.X - currentStonePos.X, 0, data.position.Z - currentStonePos.Y);
                            int dist = (int)distance.Length;

                            cipherFont.Text = $"{data.position.X - spawn.X}n,  {data.position.Y}',  {data.position.Z - spawn.Z}w  |  {dist}m";
                        }

                        normalFont.Text = "";
                    }

                    currentMousedWaystone = data;

                    return;
                }
            }
        }

        currentMousedWaystone = null;
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        dragging = false;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (bounds.IsInAllBounds(obj))
        {
            dragging = true;
            obj.Handled = true;

            if (currentMousedWaystone != null)
            {
                currentSelectedWaystone = currentMousedWaystone;
                currentMousedWaystone = null;
                AddTeleportButton();
                MainAPI.Capi.Gui.PlaySound("menubutton_down");
            }
        }
        ;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (currentSelectedWaystone != null)
        {
            Vector2d pos = new(currentSelectedWaystone!.position.X, currentSelectedWaystone!.position.Z);
            Vector2i pixel = GetPixelOfPosition(pos);
            buttonBounds?.FixedPos(pixel.X - bounds.X + 32, pixel.Y - bounds.Y - 32);
            bounds.SetBounds();
        }

        // Render background
        RenderBackground(dt, shader);

        bounds.SetBounds();

        if (currentMousedWaystone != null)
        {
            accumulator.Add(dt);
            while (accumulator.Consume())
            {
                if (cipherFont.Text.Length > 0)
                {
                    // Move first char of cipher font to end of normal font.
                    char c = cipherFont.Text[0];
                    cipherFont.Text = cipherFont.Text[1..];
                    normalFont.Text += c;
                    MainAPI.Capi.Gui.PlaySound("tick", true);
                }
            }
        }

        if (currentMousedWaystone != null)
        {
            timeMoused = Math.Clamp(timeMoused + dt, 0, 1);
            shader.BindTexture(IconBg, "tex2d");
            shader.Uniform("color", new Vector4(0, 1, 0.7f, timeMoused));
            Vector2d position = new(currentMousedWaystone.position.X, currentMousedWaystone.position.Z);
            Vector2i pixel = GetPixelOfPosition(position);
            RenderTools.RenderQuad(shader, pixel.X - 32, pixel.Y - 32, 64, 64);
            shader.Uniform("color", Vector4.One);
        }

        if (currentSelectedWaystone != null)
        {
            shader.BindTexture(IconBg, "tex2d");
            shader.Uniform("color", new Vector4(0.2f, 1, 0.5f, 1));
            Vector2d position = new(currentSelectedWaystone.position.X, currentSelectedWaystone.position.Z);
            Vector2i pixel = GetPixelOfPosition(position);
            pixel = Vector2i.Clamp(pixel, new Vector2i(bounds.X, bounds.Y), new Vector2i(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            RenderTools.RenderQuad(shader, pixel.X - 32, pixel.Y - 32, 64, 64);
            shader.Uniform("color", Vector4.One);
        }

        shader.BindTexture(ObeliskTex, "tex2d");

        foreach (WaystoneData data in waystoneData)
        {
            Vector2d waystonePos = new(data.position.X, data.position.Z);
            Vector2i pixelPos = GetPixelOfPosition(waystonePos);

            pixelPos = Vector2i.Clamp(pixelPos, new Vector2i(bounds.X, bounds.Y), new Vector2i(bounds.X + bounds.Width, bounds.Y + bounds.Height));

            RenderTools.RenderQuad(shader, pixelPos.X - 32, pixelPos.Y - 32, 64, 64);
        }

        Vector2i currentPixelPos = GetPixelOfPosition(currentStonePos);
        currentPixelPos = Vector2i.Clamp(currentPixelPos, new Vector2i(bounds.X, bounds.Y), new Vector2i(bounds.X + bounds.Width, bounds.Y + bounds.Height));
        shader.Uniform("color", new Vector4(0.5f, 6, 0.5f, 1));
        RenderTools.RenderQuad(shader, currentPixelPos.X - 32, currentPixelPos.Y - 32, 64, 64);
        shader.Uniform("color", Vector4.One);

        if (currentMousedWaystone != null)
        {
            Vector2d position = new(currentMousedWaystone.position.X, currentMousedWaystone.position.Z);
            Vector2i pixel = GetPixelOfPosition(position);
            float advance = normalFont.RenderLine(pixel.X + 32, pixel.Y, shader, 0, true);
            cipherFont.RenderLine(pixel.X + 32, pixel.Y, shader, advance, true);
        }
    }

    public void RenderBackground(float dt, MareShader shader)
    {
        MareShader cogs = MareShaderRegistry.Get("cogs");
        cogs.Use();

        cogs.Uniform("blocksPerPixel", blocksPerPixel);
        cogs.Uniform("time", MainAPI.Capi.ElapsedMilliseconds / 1000f);
        cogs.Uniform("resolution", new Vector2(bounds.Width, bounds.Height));

        Vector2 actualOffset = mouseOffset;
        actualOffset.Y = -actualOffset.Y;

        cogs.Uniform("offset", actualOffset); // Pixel offset of render.
        cogs.Uniform("renderOffset", new Vector2(bounds.X, bounds.Y)); // Offset where rendering starts.

        RenderTools.RenderQuad(cogs, bounds.X, bounds.Y, bounds.Width, bounds.Height);

        shader.Use();
    }

    public Vector2i GetPixelOfPosition(Vector2d position)
    {
        // Mouse offset is in pixels. By multiplying this by the blocks per pixel the new center pos can be calculated.
        Vector2d centerPos = initialPos + (mouseOffset * blocksPerPixel);

        Vector2d offset = position - centerPos;

        // Divide back to actual position.
        return new Vector2i((int)(bounds.X + (bounds.Width / 2) + (offset.X / blocksPerPixel)), (int)(bounds.Y + (bounds.Height / 2) + (offset.Y / blocksPerPixel)));
    }
}