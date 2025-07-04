using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{
    [SerializeField] private string terrainName = "Default";   // Name of the terrain type
    [SerializeField] private Color gizmoColour = Color.green;  // Color used for gizmos
    [SerializeField] private bool walkable = true;             // Can units walk on the terrain?
    [SerializeField] private int movementCost = 1;            // Movement cost for the terrain types

    [Header("Visual Representation")]
    [SerializeField] private GameObject terrainPrefab;         // Prefab to instantiate for this terrain
    [SerializeField] private bool showGizmos = true;           // Whether to show gizmos in editor
    [SerializeField] private Vector3 prefabOffset = Vector3.zero; // Offset for prefab positioning
    [SerializeField] private Vector3 prefabScale = Vector3.one;   // Scale for the prefab

    // Public properties
    public string TerrainName => terrainName;
    public Color GizmoColour => gizmoColour;
    public bool Walkable => walkable;
    public int MovementCost => movementCost;
    public GameObject TerrainPrefab => terrainPrefab;
    public bool ShowGizmos => showGizmos;
    public Vector3 PrefabOffset => prefabOffset;
    public Vector3 PrefabScale => prefabScale;
}