using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SelectingBuildingButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    private BuildingPlacer buildingPlacer;
    private BuildingData buildingDataForButton;

    public void SetUp(BuildingData buildingData, BuildingPlacer placer)
    {
        buildingDataForButton = buildingData;
        buildingPlacer = placer;

        buttonText.text = buildingDataForButton.BuildingName;

        // Clear any existing listeners to prevent duplicates
        button.onClick.RemoveAllListeners();

        // Hook up the click to start placing this specific building
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