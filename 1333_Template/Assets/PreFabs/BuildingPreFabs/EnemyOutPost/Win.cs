using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    // This runs automatically when the object is destroyed
    private void OnDestroy()
    {
        // If the game is running (not in editor or paused), load the Win scene
        if (Application.isPlaying)
        {
            SceneManager.LoadScene("Win");
        }
    }
}
