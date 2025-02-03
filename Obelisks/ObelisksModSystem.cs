using MareLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Obelisks;

public class RuneAttribute : ClassAttribute
{
    public string itemCode;

    public RuneAttribute(string itemCode)
    {
        this.itemCode = itemCode;
    }
}

public class GlyphAttribute : ClassAttribute
{
    public string itemCode;

    public GlyphAttribute(string itemCode)
    {
        this.itemCode = itemCode;
    }
}

[GameSystem]
public class ObelisksSystem : NetworkedGameSystem
{
    public readonly Dictionary<string, Type> glyphTypes = new();
    public readonly Dictionary<string, Type> glyphItemCodesToType = new();

    public readonly Dictionary<string, Type> runeTypes = new();
    public readonly Dictionary<string, Type> runeItemCodesToType = new();

    public ObelisksSystem(bool isServer, ICoreAPI api) : base(isServer, api, "obelisks")
    {
    }

    public override void OnStart()
    {
        base.OnStart();

        if (!isServer)
        {
            MareShaderRegistry.AddShader("obelisks:obeliskopaque", "obelisks:obeliskopaque", "obeliskopaque");
            MareShaderRegistry.AddShader("obelisks:runeoit", "obelisks:runeoit", "runeoit");
        }

        (Type, GlyphAttribute)[] glyphClasses = AttributeUtilities.GetAllAnnotatedClasses<GlyphAttribute>();
        (Type, RuneAttribute)[] runeClasses = AttributeUtilities.GetAllAnnotatedClasses<RuneAttribute>();

        foreach ((Type type, GlyphAttribute attr) in glyphClasses)
        {
            glyphTypes.Add(type.Name, type);
            glyphItemCodesToType.Add(attr.itemCode, type);
        }

        foreach ((Type type, RuneAttribute attr) in runeClasses)
        {
            runeTypes.Add(type.Name, type);
            runeItemCodesToType.Add(attr.itemCode, type);
        }
    }

    public bool TryGetGlyphType(ItemStack stack, [NotNullWhen(true)] out Type? type)
    {
        string code = stack.Collectible.Code;
        return glyphItemCodesToType.TryGetValue(code, out type);

    }

    public bool TryGetRuneType(ItemStack stack, [NotNullWhen(true)] out Type? type)
    {
        string code = stack.Collectible.Code;
        return runeItemCodesToType.TryGetValue(code, out type);
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {

    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {

    }

    public override void OnClose()
    {
        ObeliskGui.ClearCache();
    }
}

/// <summary>
/// This mod mostly operates from game systems, not the mod class.
/// </summary>
public class ObelisksModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {

    }

    public override void StartServerSide(ICoreServerAPI api)
    {

    }

    public override void StartClientSide(ICoreClientAPI api)
    {

    }
}