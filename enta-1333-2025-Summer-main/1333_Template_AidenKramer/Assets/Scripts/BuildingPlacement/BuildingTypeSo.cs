using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypeSo", menuName = "ScriptableObjects/BuildingTypes")]
public class BuildingTypeSo : ScriptableObject
{   // Start is called before the first frame update




    public List<BuildingData> Buildings = new();


}
[System.Serializable]

public class BuildingData
{
    public GameObject buildingPrefab;
    public string BuildingName;
    public string BuildingIcon;
}