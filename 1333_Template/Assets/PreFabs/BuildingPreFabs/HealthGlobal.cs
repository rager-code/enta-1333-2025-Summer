using UnityEngine;
using UnityEngine.UI;

public class SimpleHealth : MonoBehaviour
{
    // Health system variables
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Camera mCamera;
    [SerializeField] private Transform healthBarTransform; // Reference to the health bar's transform

    // Easy access properties to check health status from other scripts
    public float Health => currentHealth;
    public bool IsDead => currentHealth <= 0;

    // Initialize the health system
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthSlider();

        // Try to find a camera if none is assigned
        if (mCamera == null)
        {
            mCamera = Camera.main;
            if (mCamera == null)
            {
                mCamera = FindObjectOfType<Camera>();
            }
        }
    }

    // Handle input and updates every frame
    private void Update()
    {
        // Testing keys - P to take damage, O to heal
        if (Input.GetKeyUp(KeyCode.P))
        {
            TakeDamage(20);
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            Heal(50);
        }

        EnemyInCastle();
    }

    // Update after everything else to ensure health bar faces camera correctly
    private void LateUpdate()
    {
        LookAtCamera();
    }

    // Make the health bar always face the camera so players can read it
    private void LookAtCamera()
    {
        if (mCamera != null && healthBarTransform != null)
        {
            // Calculate direction from health bar to camera and face that way
            Vector3 directionToCamera = mCamera.transform.position - healthBarTransform.position;
            healthBarTransform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    // Reduce health when taking damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Die if health reaches zero or below
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthSlider();
    }

    // Handle death - destroy the game object
    public void Die()
    {
        Destroy(gameObject);
        Debug.Log($"Dead Unit");
    }

    // Restore health but don't exceed maximum
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthSlider();
    }

    // Update the health bar slider to show current health percentage
    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    // Special damage for when enemies reach the castle
    public void EnemyInCastle()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            TakeDamage(50);
        }
    }
}