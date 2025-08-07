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

    // Gets everything ready when the game starts up
    private void Awake()
    {
        // Initialize grid and reset the pathfinder when the game starts
        gridManager.InitializeGrid();
        pathFinder.ResetField();

        //commandTargetPath.ResetField();//new
    }

    // Checks for input every frame
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
    
}