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

    public EnemyBarracksSpawner enemyBarracksSpawner;
    // Runs once when the object is initialized
    private void Start()
    {
        if (enemyBarracksSpawner == null)
        {
            enemyBarracksSpawner = FindAnyObjectByType<EnemyBarracksSpawner>();
        }
        shouldTakeDamageFromEnemyDeaths = false;

        myHealth = GetComponent<SimpleHealth>();
        // Save the current number of enemies at the start
        previousEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;
    }

    [Header("Building Configuration")]
    public bool shouldTakeDamageFromEnemyDeaths = false; // Set to true only on the target building

    // Runs every frame
    private void Update()
    {
        // For testing: press H to deal 50 damage to this building
        if (Input.GetKeyDown(KeyCode.H))
        {
            myHealth.TakeDamage(50f);
            Debug.Log("DamageTaken");
        }

        // Periodically check if any enemies were destroyed
        if (trackEnemyDestruction && Time.time - lastCheckTime >= checkInterval)
        {
            CheckForEnemyDestruction();
            lastCheckTime = Time.time;
        }

    }

    // Compares current and previous enemy counts to see if any were destroyed
    private void CheckForEnemyDestruction()
    {
        int currentEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;

        // If fewer enemies now, figure out how many were destroyed
        if (currentEnemyCount < previousEnemyCount)
        {
            int enemiesDestroyed = previousEnemyCount - currentEnemyCount;
            OnEnemyUnitsDestroyed(enemiesDestroyed);

        }

        // Update the count for the next check
        previousEnemyCount = currentEnemyCount;
    }

    // Called when enemy units are confirmed to be destroyed
    private void OnEnemyUnitsDestroyed(int count)
    {
        Debug.Log($"[BuildingHealthAndDamage] {count} enemy unit(s) were destroyed!");

        // reduce building health when enemies are destroyed
        if (myHealth != null)
        {
           if (enemyBarracksSpawner.castleEnemyDamage == true)
           {
                myHealth.TakeDamage(15f);
                Debug.Log($"Current castle health: {myHealth.Health}");
                Debug.Log($"BoolCastleDamage: {enemyBarracksSpawner.castleEnemyDamage}");
                enemyBarracksSpawner.castleEnemyDamage = false;
           }
        }
    }
}