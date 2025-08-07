using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public static UnitManager Instance;

    public List<UnitInstance> allUnitsList = new();
    public List<UnitInstance> selectedUnits = new();

    // Sets up the singleton when this object wakes up
    void Awake()
    {
        Instance = this;
    }

    // Clears all selected units and makes them stop glowing or whatever
    public void DeselectAll()
    {
        foreach (var unit in selectedUnits)
            unit.Deselect();
        selectedUnits.Clear();
    }

    // Adds a unit to the selected group when you drag over it
    public void DragSelect(UnitInstance unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.Select();
        }
    }
}