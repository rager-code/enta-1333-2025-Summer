using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlaceMentUi : MonoBehaviour
{
    [SerializeField] private RectTransform LayoutGroupParent;
    [SerializeField] private SelectingBuildingButton ButtonPrefab;
    [SerializeField] private BuildingTypeSo BuildingData;
    [SerializeField] private BuildingPlacer buildingPlacer; // Add reference to BuildingPlacer

    private void Start()
    {
        // Find BuildingPlacer if not assigned in inspector
        if (buildingPlacer == null)
        {
            buildingPlacer = FindObjectOfType<BuildingPlacer>();
        }

        if (buildingPlacer == null)
        {
            Debug.LogError("BuildingPlacer not found! Please assign it in the inspector or ensure one exists in the scene.");
            return;
        }

        foreach (BuildingData t in BuildingData.Buildings)
        {
            SelectingBuildingButton button = Instantiate(ButtonPrefab, LayoutGroupParent);
            button.SetUp(t, buildingPlacer); // Pass the BuildingPlacer reference
        }
    }
}