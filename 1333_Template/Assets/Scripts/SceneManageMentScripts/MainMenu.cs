using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Playing the game
    public void  PlayGame()
    {

        LoadingSceenManager.instance.SwitchToScene(1);
       
    }
    //Quiting the game 
    public void QuitGame()
    {
        Debug.Log("Game Closed");
        Application.Quit();
    }
    //Loading the game 
    public void LoadingScreen()
    {

        SceneManager.LoadScene("AidenRTSScene");
    }
    //Restarting the game back to the main menu
    public void RestartGame() 
    {
        SceneManager.LoadScene("Main Menu");
    }


}
   

