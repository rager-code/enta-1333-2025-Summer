using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    [SerializeField] private string sceneName = "Win";

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            SceneManager.LoadScene("Win");
        }
    }
}
