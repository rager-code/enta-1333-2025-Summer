using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;   // Manages the grid system
    [SerializeField] private UnitManager unitManager;   // Manages units on the grid
    [SerializeField] private VisualTargetPath pathFinder;   // Handles visual pathfinding
    [SerializeField] private CommandTargetPath commandTargetPath;//new
    
    
   
    [Header("Sounds")]
    //public soundsNames Test;
   
    public AudioClip[] soundEffects;
    private void Awake()
    {
        // Initialize grid and reset the pathfinder when the game starts
        gridManager.InitializeGrid();
        pathFinder.ResetField();
       
        //commandTargetPath.ResetField();//new
    }

    private void Update()
    {
       

        if (Input.GetKeyUp(KeyCode.N))
        {
            SceneManager.LoadScene("Win");
        }

        if (Input.GetKeyUp(KeyCode.M))
        {
            SceneManager.LoadScene("Lose");
        }
    }
    /*
    public void PlaySound(soundsNames name)
    {
       
            AudioSource.PlayClipAtPoint(soundEffects[(int)name], transform.position);
        
        
    }
    public void SoundBegin()
    {
       

    }
    public void Start()
    {
        PlaySound(soundsNames.Music);
        PlaySound(soundsNames.BackGroundSounds);
    }
    */
}