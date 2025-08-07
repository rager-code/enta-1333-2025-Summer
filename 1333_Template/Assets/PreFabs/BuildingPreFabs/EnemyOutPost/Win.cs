using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            SceneManager.LoadScene("Win");
        }
    }
}
