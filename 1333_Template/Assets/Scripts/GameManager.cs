using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;   // Manages the grid system
    [SerializeField] private UnitManager unitManager;   // Manages units on the grid
    [SerializeField] private VisualTargetPath pathFinder;   // Handles visual pathfinding
    [SerializeField] private CommandTargetPath commandTargetPath;//new
    private void Awake()
    {
        // Initialize grid and reset the pathfinder when the game starts
        gridManager.InitializeGrid();
        pathFinder.ResetField();

        //commandTargetPath.ResetField();//new
    }

    private void Update()
    {
        // resets grid and player and enemy
        if (Input.GetKeyDown(KeyCode.T))
        {
            gridManager.InitializeGrid();
            pathFinder.ResetField();

            //commandTargetPath.ResetField();
        }
    }
}