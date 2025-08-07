using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutPostHealth : MonoBehaviour
{
    private SimpleHealth playerHealth;
    private int previousEnemyCount = 0;

    // Track when enemies are destroyed
    [Header("Enemy Destruction Tracking")]
    public bool trackEnemyDestruction = true;
    public float checkInterval = 0.5f; // How often to check for enemy count changes
    private float lastCheckTime = 0f;

    [Header("Damage Settings")]
    public int enemiesNeededForDamage = 2; // Number of enemies that need to die before taking damage
    private int accumulatedDestroyedEnemies = 0; // Track destroyed enemies that haven't triggered damage yet

    // Called when the script starts
    private void Start()
    {
        playerHealth = GetComponent<SimpleHealth>();
        // Get the initial number of enemies in the scene
        previousEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;
    }

    // Runs every frame
    private void Update()
    {
        // If health is zero or below, load the Win scene
        if (playerHealth.currentHealth <= 0)
        {
            Debug.LogError("Win Scene-------------");
            SceneManager.LoadScene("Win");
        }

        // Debug key to manually apply damage
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerHealth.TakeDamage(10f);
            Debug.Log("DamageTaken");
        }

        // Check for destroyed enemies on a timer
        if (trackEnemyDestruction && Time.time - lastCheckTime >= checkInterval)
        {
            CheckForEnemyDestruction();
            lastCheckTime = Time.time;
        }
    }

    // Compares enemy count to see if any have been destroyed
    private void CheckForEnemyDestruction()
    {
        int currentEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;

        if (currentEnemyCount < previousEnemyCount)
        {
            int enemiesDestroyed = previousEnemyCount - currentEnemyCount;
            OnEnemyUnitsDestroyed(enemiesDestroyed);
        }

        previousEnemyCount = currentEnemyCount;
    }

    // Called when enemy units have been destroyed
    private void OnEnemyUnitsDestroyed(int count)
    {
        Debug.Log($"[BuildingHealthAndDamage] {count} enemy unit(s) were destroyed!");

        // Add to the count of enemies that were destroyed
        accumulatedDestroyedEnemies += count;

        // If we've hit the damage threshold, apply damage
        if (accumulatedDestroyedEnemies >= enemiesNeededForDamage)
        {
            int damageInstances = accumulatedDestroyedEnemies / enemiesNeededForDamage;

            // Apply damage for each full set of required enemy deaths
            for (int i = 0; i < damageInstances; i++)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(25f);
                    Debug.Log($"OutPost took damage! Current health: {playerHealth.Health}");
                }
            }

            // Keep leftover enemies that didn't reach the threshold
            accumulatedDestroyedEnemies = accumulatedDestroyedEnemies % enemiesNeededForDamage;
        }

        Debug.Log($"Accumulated destroyed enemies: {accumulatedDestroyedEnemies}/{enemiesNeededForDamage}");

        
    }
}
