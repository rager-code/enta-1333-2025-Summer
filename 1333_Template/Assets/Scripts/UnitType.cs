using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType :ScriptableObject
{
    [SerializeField] private int maxHp = 1;
    [SerializeField] private int moveSpeed = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private int defence = 1;
    [SerializeField] private AttackType attackType;
    [SerializeField] private int range = 1;
    [SerializeField] private int width;
    [SerializeField] private int height;


    public int Width => width;
    public int Height => height;



    public int MaxHp => maxHp;
    public float MoveSpeed => moveSpeed;
    public int Damage => damage;
    public int Defence => defence;
    public AttackType AttackType => attackType;
    public int Range => range;
}
