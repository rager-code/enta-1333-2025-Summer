using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlaceMentUi : MonoBehaviour
{
    // UI setup references
    [SerializeField] private RectTransform LayoutGroupParent;    // Where to put the building buttons
    [SerializeField] private SelectingBuildingButton ButtonPrefab;  // Template for building buttons
    [SerializeField] private BuildingTypeSo BuildingData;        // All the building types we can build
    [SerializeField] private BuildingPlacer buildingPlacer;     // The system that handles placing buildings

    // Create all the building selection buttons when we start
    private void Start()
    {
        // Find BuildingPlacer if we forgot to assign it in the inspector
        if (buildingPlacer == null)
        {
            buildingPlacer = FindObjectOfType<BuildingPlacer>();
        }

        if (buildingPlacer == null)
        {
            Debug.LogError("BuildingPlacer not found! Please assign it in the inspector or ensure one exists in the scene.");
            return;
        }

        // Create a button for each building type we have
        foreach (BuildingData t in BuildingData.Buildings)
        {
            SelectingBuildingButton button = Instantiate(ButtonPrefab, LayoutGroupParent);
            button.SetUp(t, buildingPlacer); // Set up the button with the building info and placer
        }
    }
}