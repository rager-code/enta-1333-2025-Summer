using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplierManager : MonoBehaviour
{
    [SerializeField] protected Multipliers multipliers;

    [Header("Current Stats")]
    [SerializeField] private float currentResourceRate = 10f;
    [SerializeField] private int currentDamage = 50;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int currentArmor = 5;

    [Header("Base Stats (Original)")]
    [SerializeField] private float baseResourceRate = 10f;
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private int baseArmor = 5;

    private List<GameObject> nearbyObjects = new List<GameObject>();

    public void Start()
    {
        // Apply multipliers at start
        if (multipliers != null)
        {
            ApplyMultipliers();

            // If this affects nearby objects, start checking for them
            if (multipliers.effectRadius > 0f)
            {
                InvokeRepeating(nameof(UpdateNearbyObjects), 1f, 2f);
            }
        }
    }

    // Apply the multipliers to this object's stats
    public void ApplyMultipliers()
    {
        if (multipliers == null) return;

        // Reset to base stats first
        currentResourceRate = baseResourceRate;
        currentDamage = baseDamage;
        currentHealth = baseHealth;
        currentArmor = baseArmor;

        // Apply the bonuses
        multipliers.ApplyBonuses(ref currentResourceRate, ref currentDamage, ref currentHealth, ref currentArmor);

        Debug.Log($"Applied multipliers - Resource: {currentResourceRate}, Damage: {currentDamage}, Health: {currentHealth}, Armor: {currentArmor}");
    }

    // Update nearby objects that should receive buffs
    void UpdateNearbyObjects()
    {
        if (multipliers == null || multipliers.effectRadius <= 0f) return;

        // Clear previous list
        nearbyObjects.Clear();

        // Find all colliders within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, multipliers.effectRadius);

        foreach (Collider col in colliders)
        {
            if (col.gameObject == this.gameObject) continue; // Skip self

            // Check if we should affect this object
            bool shouldAffect = false;

            if (multipliers.affectsNearbyBuildings && col.CompareTag("Building"))
                shouldAffect = true;
            else if (multipliers.affectsNearbyUnits && col.CompareTag("Unit"))
                shouldAffect = true;

            if (shouldAffect)
            {
                nearbyObjects.Add(col.gameObject);

                // Apply buff to nearby object
                MultiplierManager nearbyManager = col.GetComponent<MultiplierManager>();
                if (nearbyManager != null)
                {
                    nearbyManager.ApplyExternalBuff(multipliers);
                }
            }
        }
    }

    // Apply external buff from another building
    public void ApplyExternalBuff(Multipliers externalMultipliers)
    {
        if (externalMultipliers == null) return;

        // You could stack multiple external buffs here
        float tempResource = currentResourceRate;
        int tempDamage = currentDamage;
        int tempHealth = currentHealth;
        int tempArmor = currentArmor;

        externalMultipliers.ApplyBonuses(ref tempResource, ref tempDamage, ref tempHealth, ref tempArmor);

        // Update current stats with external buff
        currentResourceRate = tempResource;
        currentDamage = tempDamage;
        currentHealth = tempHealth;
        currentArmor = tempArmor;
    }

    // Get current stats for other systems to use
    public float GetResourceRate() => currentResourceRate;
    public int GetDamage() => currentDamage;
    public int GetHealth() => currentHealth;
    public int GetArmor() => currentArmor;

    // Change the multiplier at runtime
    public void SetMultipliers(Multipliers newMultipliers)
    {
        multipliers = newMultipliers;
        ApplyMultipliers();
    }

    // Visual debug in scene view
    void OnDrawGizmosSelected()
    {
        if (multipliers != null && multipliers.effectRadius > 0f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, multipliers.effectRadius);
        }
    }
}