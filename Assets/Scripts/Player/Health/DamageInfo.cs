using UnityEngine;
using System;

/// <summary>
/// Damage information structure
/// </summary>
public struct DamageInfo
{
    public float Amount;
    public DamageType Type;
    public bool BypassArmor;
}

/// <summary>
/// Damage type enum for resistance calculations
/// </summary>
public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison
}
