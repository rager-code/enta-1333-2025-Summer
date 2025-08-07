using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Multipliers", menuName = "Scriptable Objects/Multipliers")]
public class Multipliers : ScriptableObject
{
    [Header("Resource Multipliers")]
    [Range(1f, 5f)]
    public float resourceProductionMultiplier = 1f;

    [Range(1f, 3f)]
    public float resourceCapacityMultiplier = 1f;

    [Range(0.5f, 1f)]
    public float resourceCostReduction = 1f; // 0.8 = 20% cost reduction

    [Header("Combat Stats")]
    [Range(0, 200)]
    public int damageBonus = 0;

    [Range(0, 1000)]
    public int healthBonus = 0;

    [Range(1f, 2f)]
    public float criticalHitMultiplier = 1f;

    [Range(1f, 2f)]
    public float attackSpeedMultiplier = 1f;

    [Header("Defense Bonuses")]
    [Range(0, 100)]
    public int armorBonus = 0;

    [Range(1f, 3f)]
    public float repairSpeedMultiplier = 1f;

    [Header("Special Bonuses")]
    public bool providesShield = false;

    [Range(0f, 100f)]
    public float shieldStrength = 0f;

    [Header("Area of Effect")]
    [Range(0f, 20f)]
    public float effectRadius = 0f; // 0 means only affects this building

    public bool affectsNearbyBuildings = false;
    public bool affectsNearbyUnits = false;

    [Header("Stacking Rules")]
    public bool stacksWithOtherMultipliers = true;
    public int maxStackCount = 1;

    // Method to apply bonuses to a target
    public void ApplyBonuses(ref float resource, ref int damage, ref int health, ref int armor)
    {
        resource *= resourceProductionMultiplier;
        damage += damageBonus;
        health += healthBonus;
        armor += armorBonus;
    }

    // Method to get total bonus data
    public MultiplierData GetMultiplierData()
    {
        return new MultiplierData
        {
            resourceMult = resourceProductionMultiplier,
            damageBonus = damageBonus,
            healthBonus = healthBonus,
            armorBonus = armorBonus,
            radius = effectRadius,
            affectsBuildings = affectsNearbyBuildings,
            affectsUnits = affectsNearbyUnits
        };
    }
}

[System.Serializable]
public struct MultiplierData
{
    public float resourceMult;
    public int damageBonus;
    public int healthBonus;
    public int armorBonus;
    public float radius;
    public bool affectsBuildings;
    public bool affectsUnits;
}