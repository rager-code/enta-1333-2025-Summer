using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // When this object gets destroyed, trigger the game over sequence
    private void OnDestroy()
    {
        // Only load the game over scene if we're actually playing the game
        // (not when exiting play mode in the editor)
        if (Application.isPlaying)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}