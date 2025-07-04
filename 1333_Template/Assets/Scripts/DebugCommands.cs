using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    // Start is called before the first frame update
    
    
    void OnEnable()
    {
        DebugLogConsole.AddCommand<int>("HelloWorld", "Prints a message to the console", HelloWorld);

        


    }

    
    private void HelloWorld(int obj)
    {
        Debug.Log("HelloWorld");

    }
    

}
