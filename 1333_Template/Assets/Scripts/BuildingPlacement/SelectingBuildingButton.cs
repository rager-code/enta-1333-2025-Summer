using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SelectingBuildingButton : MonoBehaviour
{
    // UI components that make up this building selection button
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    // References to the building system
    private BuildingPlacer buildingPlacer;
    private BuildingData buildingDataForButton;

    // Set up this button with a specific building and connect it to the building placer
    public void SetUp(BuildingData buildingData, BuildingPlacer placer)
    {
        buildingDataForButton = buildingData;
        buildingPlacer = placer;

        // Update the button's appearance with the building info
        buttonText.text = buildingDataForButton.BuildingName;
        buttonImage.sprite = buildingDataForButton.BuildingIcon;

        // Clear any existing listeners to prevent duplicates
        button.onClick.RemoveAllListeners();

        // When clicked, tell the building placer to start placing this building
        button.onClick.AddListener(() => {
            if (buildingPlacer != null && buildingDataForButton.buildingPrefab != null)
            {
                buildingPlacer.StartPlacingBuilding(buildingDataForButton.buildingPrefab);
            }
            else
            {
                Debug.LogError("BuildingPlacer or building prefab is null!");
            }
        });
    }
}