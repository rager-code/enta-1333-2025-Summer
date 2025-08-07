using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{


    [SerializeField] protected UnitType unitType;



    //public abstract void MoveTo(GridNode targetNode);



    // Gets how wide this unit is, falls back to 1 if no unit type assigned
    public virtual int Width => unitType != null ? unitType.Width : 1;

    // Gets how tall this unit is, falls back to 1 if no unit type assigned
    public virtual int Height => unitType != null ? unitType.Height : 1;

    // Override this to tell a unit where to go
    public virtual void MoveTo(GridNode targetNode)
    {


    }

    // Override this to handle how the unit actually moves each frame
    public virtual void DoMove()
    {

    }

    // Override this to add stuff that happens every game tick
    public virtual void PerTick() { }

}