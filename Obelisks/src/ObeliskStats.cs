using System;

namespace Obelisks;

/// <summary>
/// Passed into inscriptions to set stats.
/// </summary>
public class ObeliskStats
{
    public int Potentia { get; set; } = 0;
    public int maxPotentia = 10_000;
    public float powerMultiplier = 1;
    public float aoeMultiplier = 1;
    public float potentiaCostMultiplier = 1;

    public float PotentiaPercent => Potentia / (float)maxPotentia;

    public ObeliskStats(int currentPotentia)
    {
        Potentia = currentPotentia;
    }

    public void AddPotentia(int amount)
    {
        Potentia += amount;
        Potentia = Math.Clamp(Potentia, 0, maxPotentia);
    }
}