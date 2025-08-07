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

    // Gets the name of this terrain type
    public string TerrainName => terrainName;

    // Gets the color to draw this terrain in the editor
    public Color GizmoColour => gizmoColour;

    // Returns true if units can walk on this terrain
    public bool Walkable => walkable;

    // How much it costs to move through this terrain
    public int MovementCost => movementCost;

    // The 3D object to place for this terrain type
    public GameObject TerrainPrefab => terrainPrefab;

    // Whether to show colored squares in the editor
    public bool ShowGizmos => showGizmos;

    // How much to offset the 3D object from the grid position
    public Vector3 PrefabOffset => prefabOffset;

    // How big to make the 3D object
    public Vector3 PrefabScale => prefabScale;
}