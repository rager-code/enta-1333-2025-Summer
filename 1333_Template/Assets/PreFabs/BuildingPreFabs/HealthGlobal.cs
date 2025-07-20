using UnityEngine;
using UnityEngine.UI;

public class SimpleHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private Slider healthSlider;
    //[SerializeField] private Camera mCamera;
    //[SerializeField] private Transform transformTarget;
    //[SerializeField] private Vector3 offSet;
    public float Health => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthSlider();
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
        //HealthBar To look at camera
        //transform.rotation = mCamera.transform.rotation;
        //transformTarget.position = transform.position + offSet;
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
}