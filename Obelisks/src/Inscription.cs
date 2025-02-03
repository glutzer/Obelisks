using OpenTK.Mathematics;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Obelisks;

/// <summary>
/// Main behavior.
/// </summary>
public abstract class Inscription
{
    public readonly BlockEntityObelisk obelisk;
    public readonly bool isServer;
    public readonly BlockPos pos;
    public readonly string code;

    public virtual Vector4 Color => Vector4.One;
    public virtual int MaxPower => 50;
    public int currentPower = 0;
    public float PowerPercent => currentPower / (float)MaxPower;

    public readonly string itemCode = null!;

    public readonly bool rune;

    protected Inscription(BlockEntityObelisk obelisk, BlockPos pos, IWorldAccessor world)
    {
        this.obelisk = obelisk;
        isServer = world.Side == EnumAppSide.Server;
        this.pos = pos;
        code = GetType().Name;

        object[] attrs = GetType().GetCustomAttributes(false);

        for (int i = 0; i < attrs.Length; i++)
        {
            if (attrs[i] is GlyphAttribute glyphAttr)
            {
                rune = false;
                itemCode = glyphAttr.itemCode;
                break;
            }

            if (attrs[i] is RuneAttribute runeAttr)
            {
                rune = true;
                itemCode = runeAttr.itemCode;
                break;
            }
        }
    }

    public virtual void OnInteract(IPlayer player)
    {
    }

    public virtual void OnAdded()
    {

    }

    public virtual void OnRemoved()
    {

    }

    public virtual void FromTreeAttributes(ITreeAttribute tree)
    {
        currentPower = tree.GetInt("power", 0);
    }

    public virtual void ToTreeAttributes(ITreeAttribute tree)
    {
        tree.SetInt("power", currentPower);
    }

    public virtual void AddPower(ItemStack stack)
    {
        if (currentPower >= MaxPower) return;
        stack.StackSize--;
        currentPower++;
    }

    public virtual void GetInfo(StringBuilder str)
    {
        str.AppendLine($"{Lang.Get("obelisks:" + code)}: {currentPower}/{MaxPower}");
    }

    public virtual void ModifyStats(ObeliskStats stats)
    {

    }

    /// <summary>
    /// When this has been inscribed onto an obelisk.
    /// </summary>
    public virtual void OnCreated()
    {

    }

    /// <summary>
    /// When the obelisk with this has been destroyed, or the inscription has been replaced.
    /// </summary>
    public virtual void OnDestroyed()
    {

    }
}