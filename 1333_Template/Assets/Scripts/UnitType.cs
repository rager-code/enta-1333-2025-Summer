using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    [SerializeField] private int maxHp = 1;
    [SerializeField] private int moveSpeed = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private int defence = 1;
    [SerializeField] private AttackType attackType;
    [SerializeField] private int range = 1;
    [SerializeField] private int width;
    [SerializeField] private int height;

    // Just gives back how wide this unit is
    public int Width => width;

    // Just gives back how tall this unit is
    public int Height => height;

    // Returns the max health this unit can have
    public int MaxHp => maxHp;

    // How fast this unit moves around
    public float MoveSpeed => moveSpeed;

    // How much damage this unit deals
    public int Damage => damage;

    // How well this unit can take a hit
    public int Defence => defence;

    // What kind of attack this unit has
    public AttackType AttackType => attackType;

    // How far this unit can attack from
    public int Range => range;
}