using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called before the first frame update
    public int damage = 10;
    private SimpleHealth playerHealth;
    private void OnTriggerEnter(Collision collision)
    {
        Debug.Log("Collision Works OutPost");
        if (collision.gameObject.tag == "Player")
        {
            if (playerHealth == null)
            {
                playerHealth = collision.gameObject.GetComponent<SimpleHealth>();
            }
            playerHealth.TakeDamage(damage);
        }
    }
    
}
