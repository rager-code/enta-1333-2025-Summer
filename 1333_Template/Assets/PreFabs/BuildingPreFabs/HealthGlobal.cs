using UnityEngine;
using UnityEngine.UI;

public class SimpleHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Camera mCamera;
    [SerializeField] private Transform healthBarTransform; // Reference to the health bar's transform

    public float Health => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthSlider();

        // If no camera is assigned, try to find the main camera
        if (mCamera == null)
        {
            mCamera = Camera.main;
            if (mCamera == null)
            {
                mCamera = FindObjectOfType<Camera>();
            }
        }

        /*// If no health bar transform is assigned, use this transform
        if (healthBarTransform == null)
        {
            healthBarTransform = transform;
        }*/
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            TakeDamage(20);
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            Heal(50);
        }
        
        EnemyInCastle();
        // Make health bar look at camera
        
    }
    private void LateUpdate()
    {
        LookAtCamera();
    }
    private void LookAtCamera()
    {
        if (mCamera != null && healthBarTransform != null)
        {
            // Make the health bar look at the camera
            Vector3 directionToCamera = mCamera.transform.position - healthBarTransform.position;
            healthBarTransform.rotation = Quaternion.LookRotation(directionToCamera);

            // Alternative approach - billboard effect (always faces camera)
            // healthBarTransform.LookAt(mCamera.transform);

            // If you want the health bar to only rotate on Y-axis (vertical billboard)
            // Vector3 targetPosition = new Vector3(mCamera.transform.position.x, healthBarTransform.position.y, mCamera.transform.position.z);
            // healthBarTransform.LookAt(targetPosition);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthSlider();
    }

    public void Die()
    {
        Destroy(gameObject);
        Debug.Log($"Dead Unit");
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthSlider();
    }

    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }
    public void EnemyInCastle()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            TakeDamage(50);
        }
       
    }
}