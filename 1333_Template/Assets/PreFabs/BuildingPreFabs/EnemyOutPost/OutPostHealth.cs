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

    private void Start()
    {
        playerHealth = GetComponent<SimpleHealth>();
        // Initialize the enemy count
        previousEnemyCount = EnemyBarracksSpawner.GetAllActiveEnemyUnits().Count;
    }

    private void Update()
    {

        if (playerHealth.currentHealth <= 0)
        {
            Debug.LogError("Win Scene-------------");
            SceneManager.LoadScene("Win");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            playerHealth.TakeDamage(10f);
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

        // Accumulate destroyed enemies
        accumulatedDestroyedEnemies += count;

        // Check if we've reached the threshold for taking damage
        if (accumulatedDestroyedEnemies >= enemiesNeededForDamage)
        {
            // Calculate how many damage instances to apply
            int damageInstances = accumulatedDestroyedEnemies / enemiesNeededForDamage;

            // Apply damage for each complete set of enemies
            for (int i = 0; i < damageInstances; i++)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(50f);
                    Debug.Log($"OutPost took damage! Current health: {playerHealth.Health}");
                }
            }

            // Keep track of remaining enemies that didn't reach the threshold
            accumulatedDestroyedEnemies = accumulatedDestroyedEnemies % enemiesNeededForDamage;
        }

        Debug.Log($"Accumulated destroyed enemies: {accumulatedDestroyedEnemies}/{enemiesNeededForDamage}");

        // Add your other custom logic here for when enemies are destroyed
        // For example:
        // - Play destruction sound effects
        // - Award points to player
        // - Update UI
        // - Trigger special effects
    }
}