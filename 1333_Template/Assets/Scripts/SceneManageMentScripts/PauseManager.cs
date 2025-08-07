using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private static bool isPaused = false;

    public GameObject PauseMenuUI;



    void Update()
    {
        // Check for Escape key press to toggle pause state
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
                
        }
    }
    //Resumes the game
    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unpause the game
        isPaused = false ;
        Debug.Log("Game Resumed");
      

    }
    //Pauses the game
    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        isPaused = true;
        Debug.Log("Game Paused");
        
    }
    //Sets the Pas
    public void Start()
    {
        PauseMenuUI.SetActive(false);
    }
}