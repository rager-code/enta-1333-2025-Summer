using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Different states a unit can be in during the game
public enum UnitState
{
    Moving,    // Unit is walking around
    Attacking, // Unit is fighting something
    Idle,      // Unit is just chilling and waiting
}