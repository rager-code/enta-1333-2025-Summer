using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;   // Manages the grid system
    [SerializeField] private UnitManager unitManager;   // Manages units on the grid
    [SerializeField] private VisualTargetPath pathFinder;   // Handles visual pathfinding
    [SerializeField] private CommandTargetPath commandTargetPath;//new
    
    
    public enum soundsNames
    {
       spawnPlayerUnit,
       BackGroundSounds,
       Music,

    }
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
        if (Input.GetKeyUp(KeyCode.F))
        {
            PlaySound(soundsNames.spawnPlayerUnit);
        }
    }
    public void PlaySound(soundsNames name)
    {
       
            AudioSource.PlayClipAtPoint(soundEffects[(int)name], transform.position);
        
        
    }
    public void SoundBegin()
    {
       

    }
    public void Start()
    {
        
        //music here
        PlaySound(soundsNames.BackGroundSounds);
    }
}