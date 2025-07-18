using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHealthAndDamage : MonoBehaviour
{
    
    
        private SimpleHealth myHealth;

        private void Start()
        {
            myHealth = GetComponent<SimpleHealth>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
               
                    myHealth.TakeDamage(10f);
                    Debug.Log("DamageTaken");
                
            }
        }

}    
