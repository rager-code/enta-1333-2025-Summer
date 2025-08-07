using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHealthAndDamage : MonoBehaviour
{
    private SimpleHealth myHealth;
    private int previousEnemyCount = 0;

    // Track when enemies are destroyed
    [Header("Enemy Destruction Tracking")]
    public bool trackEnemyDestruction = true;
    public float checkInterval = 0.5f; // How often to check for enemy count changes
    private float lastCheckTime = 0f;

    private void Start()
    {
        myHealth = GetComponent<SimpleHealth>();
        // Initialize the enemy count
        previousEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            myHealth.TakeDamage(50f);
            Debug.Log("DamageTaken");
        }

        // Check for enemy destruction periodically
        if (trackEnemyDestruction && Time.time - lastCheckTime >= checkInterval)
        {
            CheckForEnemyDestruction();
            lastCheckTime = Time.time;
        }
    }

    private void CheckForEnemyDestruction()
    {
        int currentEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;

        // If enemy count decreased, some enemies were destroyed
        if (currentEnemyCount < previousEnemyCount)
        {
            int enemiesDestroyed = previousEnemyCount - currentEnemyCount;
            OnEnemyUnitsDestroyed(enemiesDestroyed);
        }

        previousEnemyCount = currentEnemyCount;
    }

    private void OnEnemyUnitsDestroyed(int count)
    {
        Debug.Log($"[BuildingHealthAndDamage] {count} enemy unit(s) were destroyed!");

        // Add your custom logic here for when enemies are destroyed
        // For example:
        // - Play destruction sound effects
        // - Award points to player
        // - Update UI
        // - Trigger special effects

        // Example: Check if this was due to reaching the castle
        if (myHealth != null)
        {
            myHealth.TakeDamage(50f);
            Debug.Log($"Current castle health: {myHealth.Health}");

            // You could check if health decreased recently to confirm
            // enemies reached the castle vs being destroyed by other means
        }
    }
}