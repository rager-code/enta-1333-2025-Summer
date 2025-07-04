using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    
    public static UnitManager Instance;

    public List<UnitInstance> allUnitsList = new();
    public List<UnitInstance> selectedUnits = new();

    void Awake()
    {
        Instance = this;
    }

    public void DeselectAll()
    {
        foreach (var unit in selectedUnits)
            unit.Deselect();
        selectedUnits.Clear();
    }

    public void DragSelect(UnitInstance unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.Select();
        }
    }
}
