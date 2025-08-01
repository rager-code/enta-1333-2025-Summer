using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotWalkAble : MonoBehaviour
{


    [System.Serializable]
    public class BuildingTypeLimit
    {
        public GameObject OutPost;

        [Tooltip("Optional: Set custom width/height for this building type")]
        public int buildingWidth = 4;
        public int buildingHeight = 4;

        [Tooltip("Custom unwalkable area")]
        public int unwalkableWidth = 4;
        public int unwalkableHeight = 4;

        public int maxBuildingOfType = 1;
    }

    [Header("Building Settings")]
    public GameObject buildingPrefab;
    [SerializeField] private int buildingWidth = 4;  // Default building width in grid units
    [SerializeField] private int buildingHeight = 4; // Default building height in grid units

    [Header("Custom Unwalkable Area")]
    [SerializeField] private int customUnwalkableWidth = 4;  // Custom unwalkable width
    [SerializeField] private int customUnwalkableHeight = 4; // Custom unwalkable height
    [SerializeField] private bool useCustomUnwalkableArea = false; // Toggle for custom area

    [Header("Building Type Limits")]
    [SerializeField] private BuildingTypeLimit[] buildingTypeLimits;

    [Header("Grid Reference")]
    [SerializeField] private GridManager gridManager; // Reference to the grid manager

    private GameObject currentBuilding;
    private Renderer[] buildingRenderers; // Handle multiple renderers
    private Material[] originalMaterials; // Store original materials


    // Current unwalkable dimensions for the building being placed
    private int currentUnwalkableWidth;
    private int currentUnwalkableHeight;

    // Dictionary to track placed buildings by prefab type
    private static Dictionary<GameObject, int> placedBuildingsByType = new Dictionary<GameObject, int>();

    private void Start()
    {
        // Find GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found! Please assign it in the inspector or ensure one exists in the scene.");
                return;
            }
        }

        // Set up dimensions
        if (useCustomUnwalkableArea)
        {
            currentUnwalkableWidth = customUnwalkableWidth;
            currentUnwalkableHeight = customUnwalkableHeight;
        }
        else
        {
            currentUnwalkableWidth = buildingWidth;
            currentUnwalkableHeight = buildingHeight;
        }

        // Snap to grid and set unwalkable
        StartCoroutine(SetupAfterGrid());
    }

    private IEnumerator SetupAfterGrid()
    {
        // Wait for grid to initialize
        while (gridManager != null && !gridManager.IsInitialized)
        {
            yield return null;
        }

        // Snap this building to grid
        transform.position = SnapToGrid(transform.position);

        // Set nodes unwalkable
        SetNodesUnwalkable(transform.position);
    }



    public void StartPlacingBuilding(GameObject newBuildingPrefab)
    {
        if (newBuildingPrefab == null)
        {
            Debug.LogError("Building prefab is null!");
            return;
        }



        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
        }

        buildingPrefab = newBuildingPrefab;

        // Get the specific dimensions for this building type
        GetBuildingDimensions(newBuildingPrefab, out buildingWidth, out buildingHeight);

        // Get unwalkable dimensions (can be different from visual building size)
        GetUnwalkableDimensions(newBuildingPrefab, out currentUnwalkableWidth, out currentUnwalkableHeight);

        currentBuilding = Instantiate(buildingPrefab);

        // Get all renderers in the building (including children)
        buildingRenderers = currentBuilding.GetComponentsInChildren<Renderer>();

        if (buildingRenderers.Length == 0)
        {
            Debug.LogWarning("No renderers found on building prefab!");
            return;
        }

        // Store original materials
        StoreOriginalMaterials();


    }

    // Method to set custom unwalkable area at runtime
    public void SetCustomUnwalkableArea(int width, int height)
    {
        customUnwalkableWidth = width;
        customUnwalkableHeight = height;
        useCustomUnwalkableArea = true;

        Debug.Log($"Custom unwalkable area set to {width}x{height}");
    }

    // Method to toggle custom unwalkable area
    public void EnableCustomUnwalkableArea(bool enable)
    {
        useCustomUnwalkableArea = enable;
    }



    // Get building dimensions for a specific prefab
    private void GetBuildingDimensions(GameObject prefab, out int width, out int height)
    {
        foreach (BuildingTypeLimit limit in buildingTypeLimits)
        {
            if (limit.OutPost == prefab)
            {
                width = limit.buildingWidth;
                height = limit.buildingHeight;
                return;
            }
        }
        // Use default dimensions if not found
        width = buildingWidth;
        height = buildingHeight;
    }

    // Get unwalkable dimensions for a specific prefab (can be different from visual size)
    private void GetUnwalkableDimensions(GameObject prefab, out int width, out int height)
    {
        // If using custom unwalkable area, use those values
        if (useCustomUnwalkableArea)
        {
            width = customUnwalkableWidth;
            height = customUnwalkableHeight;
            return;
        }

        // Otherwise, check building type limits for specific unwalkable dimensions
        foreach (BuildingTypeLimit limit in buildingTypeLimits)
        {
            if (limit.OutPost == prefab)
            {
                width = limit.unwalkableWidth;
                height = limit.unwalkableHeight;
                return;
            }
        }

        // Use building dimensions as fallback
        GetBuildingDimensions(prefab, out width, out height);
    }


    void StoreOriginalMaterials()
    {
        List<Material> originals = new List<Material>();

        foreach (Renderer renderer in buildingRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                originals.Add(mat);
            }
        }

        originalMaterials = originals.ToArray();
    }

    void SetNodesUnwalkable(Vector3 buildingPosition)
    {
        if (gridManager == null || !gridManager.IsInitialized) return;

        // Calculate half extents for proper centering
        float halfWidth = (currentUnwalkableWidth - 1) / 2f;
        float halfHeight = (currentUnwalkableHeight - 1) / 2f;

        // Calculate starting position by offsetting from center to create area around building center
        Vector3 startPos = buildingPosition - new Vector3(
            halfWidth * gridManager.GridSettings.NodeSize,
            0,
            halfHeight * gridManager.GridSettings.NodeSize
        );

        // Set each node in the unwalkable area to not walkable
        for (int x = 0; x < currentUnwalkableWidth; x++)
        {
            for (int y = 0; y < currentUnwalkableHeight; y++)
            {
                Vector3 nodeWorldPos = startPos + new Vector3(
                    x * gridManager.GridSettings.NodeSize,
                    0,
                    y * gridManager.GridSettings.NodeSize
                );

                GridNode node = gridManager.GetNodeFromWorldPosition(nodeWorldPos);

                if (node != null)
                {
                    node.walkable = false;
                    Debug.Log($"Set node at {node.WorldPosition} to not walkable (unwalkable area: {currentUnwalkableWidth}x{currentUnwalkableHeight})");
                }
            }
        }

        Debug.Log($"Created {currentUnwalkableWidth}x{currentUnwalkableHeight} unwalkable area centered at {buildingPosition}");
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        if (gridManager == null)
        {
            // Fallback to default grid size if no grid manager
            float gridSize = 1f;
            float offset = gridSize * 0.5f;
            float x = Mathf.Round(position.x / gridSize) * gridSize + offset;
            float z = Mathf.Round(position.z / gridSize) * gridSize + offset;
            return new Vector3(x, position.y, z);
        }

        float nodeSize = gridManager.GridSettings.NodeSize;

        // Snap to grid based on the grid manager's node size
        float x_snapped = Mathf.Round(position.x / nodeSize) * nodeSize;
        float z_snapped = Mathf.Round(position.z / nodeSize) * nodeSize;

        return new Vector3(x_snapped, position.y, z_snapped);
    }

    // Optional: Method to set building size from inspector or code
    public void SetBuildingSize(int width, int height)
    {
        buildingWidth = width;
        buildingHeight = height;
    }
}