using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
public class GridSettings : ScriptableObject
{
    //  Grid settings
    [SerializeField] private int gridSizeX = 10;     // Number of nodes along X axis
    [SerializeField] private int gridSizeY = 10;     // Number of nodes along Y axis
    [SerializeField] private float nodeSize = 1f;    // Size of each grid node
    [SerializeField] private bool useXZPlane = true; // Toggle between XZ and XY plane

    // How many grid squares wide the map is
    public int GridSizeX => gridSizeX;

    // How many grid squares tall the map is
    public int GridSizeY => gridSizeY;

    // How big each grid square is in world units
    public float NodeSize => nodeSize;

    // Whether we're using XZ plane (top-down) or XY plane (side view)
    public bool UseXZPlane => useXZPlane;
}