using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;

// This creates a right-click menu option to make new building type assets
[CreateAssetMenu(fileName = "BuildingTypeSo", menuName = "ScriptableObjects/BuildingTypes")]
public class BuildingTypeSo : ScriptableObject
{
    // List of all the different buildings the player can construct
    public List<BuildingData> Buildings = new();
}

// This holds all the info about a single building type
[System.Serializable]
public class BuildingData
{
    public GameObject buildingPrefab;    // The 3D model that gets placed in the world
    public string BuildingName;          // What the building is called
    public Sprite BuildingIcon;          // Icon to show in the UI
}