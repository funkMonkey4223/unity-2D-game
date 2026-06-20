using UnityEngine;

/// <summary>
/// Health model containing player stats and resistances
/// </summary>
[CreateAssetMenu(fileName = "HealthModel", menuName = "ScriptableObjects/HealthModel")]
public class HealthModel : ScriptableObject
{
    [SerializeField] public float MaxHP = 100f;
    [SerializeField] public float RegenPerSecond = 5f;
    [SerializeField] public float ArmorFlat = 5f;
    [SerializeField] public float ArmorPercent = 0.1f;

    [SerializeField] private float fireResist = 0.1f;
    [SerializeField] private float iceResist = 0.1f;
    [SerializeField] private float lightningResist = 0.1f;
    [SerializeField] private float poisonResist = 0.1f;

    public float GetResistance(DamageType type)
    {
        return type switch
        {
            DamageType.Fire => fireResist,
            DamageType.Ice => iceResist,
            DamageType.Lightning => lightningResist,
            DamageType.Poison => poisonResist,
            _ => 0f
        };
    }
}
