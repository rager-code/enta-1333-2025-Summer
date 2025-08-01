using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void  PlayGame()
    {
      
        //LoadingScreen();
        LoadingSceenManager.instance.SwitchToScene(1);
       
    }

    public void QuitGame()
    {
        Debug.Log("Game Closed");
        Application.Quit();
    }
    public void LoadingScreen()
    {

        SceneManager.LoadScene("AidenRTSScene");
    }
    public void RestartGame() //New
    {
        SceneManager.LoadScene("Main Menu");
    }


}
   

