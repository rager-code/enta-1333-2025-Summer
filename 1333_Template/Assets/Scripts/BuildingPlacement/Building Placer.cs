using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingPlacer : MonoBehaviour
{
    [System.Serializable]
    public class BuildingTypeLimit
    {
        public GameObject prefab;

        [Tooltip("Optional: Set custom width/height for this building type")]
        public int buildingWidth = 4;
        public int buildingHeight = 4;

        [Tooltip("Custom unwalkable area - can be different from building visual size")]
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
    private bool canPlace = false;
    private bool isPlaced = false;

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
            }
        }
    }

    void Update()
    {
        if (currentBuilding != null)
        {
            MoveBuildingWithMouse();
            CheckPlacementValidity();

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceBuilding();
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateBuilding();
            }
          
        }
    }

    public void StartPlacingBuilding(GameObject newBuildingPrefab)
    {
        if (newBuildingPrefab == null)
        {
            Debug.LogError("Building prefab is null!");
            return;
        }

        // Check if max buildings limit reached for this specific building type
        int currentCount = GetPlacedBuildingCount(newBuildingPrefab);
        int maxAllowed = GetMaxBuildingCount(newBuildingPrefab);
        if (currentCount >= maxAllowed)
        {
            Debug.Log($"Cannot place more buildings of this type. Maximum limit of {maxAllowed} reached for {newBuildingPrefab.name}.");
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

        SetBuildingColor(Color.red);
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

    // Get the maximum allowed count for a specific prefab type
    private int GetMaxBuildingCount(GameObject prefab)
    {
        foreach (BuildingTypeLimit limit in buildingTypeLimits)
        {
            if (limit.prefab == prefab)
            {
                return limit.maxBuildingOfType;
            }
        }
        return 1; // Default limit if not found in the list
    }

    // Get building dimensions for a specific prefab
    private void GetBuildingDimensions(GameObject prefab, out int width, out int height)
    {
        foreach (BuildingTypeLimit limit in buildingTypeLimits)
        {
            if (limit.prefab == prefab)
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
            if (limit.prefab == prefab)
            {
                width = limit.unwalkableWidth;
                height = limit.unwalkableHeight;
                return;
            }
        }

        // Use building dimensions as fallback
        GetBuildingDimensions(prefab, out width, out height);
    }

    // Get the count of placed buildings for a specific prefab type
    private int GetPlacedBuildingCount(GameObject prefab)
    {
        if (placedBuildingsByType.ContainsKey(prefab))
        {
            return placedBuildingsByType[prefab];
        }
        return 0;
    }

    // Increment the count for a specific building type
    private void IncrementBuildingCount(GameObject prefab)
    {
        if (placedBuildingsByType.ContainsKey(prefab))
        {
            placedBuildingsByType[prefab]++;
        }
        else
        {
            placedBuildingsByType[prefab] = 1;
        }
    }

    // Optional: Method to reset building counts (useful for level resets)
    public static void ResetBuildingCounts()
    {
        placedBuildingsByType.Clear();
    }

    // Optional: Method to get remaining buildings for a specific type
    public int GetRemainingBuildings(GameObject prefab)
    {
        int placed = GetPlacedBuildingCount(prefab);
        int maxAllowed = GetMaxBuildingCount(prefab);
        return Mathf.Max(0, maxAllowed - placed);
    }

    void RotateBuilding()
    {
        if (currentBuilding != null)
        {
            // Rotate the building 90 degrees around the Y-axis
            currentBuilding.transform.Rotate(0, 90, 0);

            // Automatically swap unwalkable dimensions when rotating (rotate the footprint)
            int temp = currentUnwalkableWidth;
            currentUnwalkableWidth = currentUnwalkableHeight;
            currentUnwalkableHeight = temp;

            Debug.Log($"Building rotated. New unwalkable area: {currentUnwalkableWidth}x{currentUnwalkableHeight}");
        }
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

    void MoveBuildingWithMouse()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            currentBuilding.transform.position = SnapToGrid(hit.point);
        }
    }

    void CheckPlacementValidity()
    {
        if (gridManager == null || !gridManager.IsInitialized)
        {
            SetBuildingColor(Color.red);
            canPlace = false;
            return;
        }

        // Check if all nodes in the unwalkable area are walkable
        bool allNodesWalkable = AreNodesWalkable(currentBuilding.transform.position);

        if (allNodesWalkable)
        {
            SetBuildingColor(Color.green);
            canPlace = true;
        }
        else
        {
            SetBuildingColor(Color.red);
            canPlace = false;
        }
    }

    bool AreNodesWalkable(Vector3 buildingPosition)
    {
        // Get the center node of the building
        GridNode centerNode = gridManager.GetNodeFromWorldPosition(buildingPosition);
        if (centerNode == null) return false;

        // Calculate starting position by offsetting from center to create area around building center
        Vector3 startPos = buildingPosition - new Vector3(
            (currentUnwalkableWidth / 2) * gridManager.GridSettings.NodeSize,
            0,
            (currentUnwalkableHeight / 2) * gridManager.GridSettings.NodeSize
        );

        // Check each node in the unwalkable area
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

                // If any node is null or not walkable, return false
                if (node == null || !node.walkable)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void SetNodesUnwalkable(Vector3 buildingPosition)
    {
        if (gridManager == null || !gridManager.IsInitialized) return;

        // Calculate starting position by offsetting from center to create area around building center
        Vector3 startPos = buildingPosition - new Vector3(
            (currentUnwalkableWidth / 2) * gridManager.GridSettings.NodeSize,
            0,
            (currentUnwalkableHeight / 2) * gridManager.GridSettings.NodeSize
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
                    Debug.Log($"Set node at {nodeWorldPos} to not walkable (unwalkable area: {currentUnwalkableWidth}x{currentUnwalkableHeight})");
                }
            }
        }
    }

    void PlaceBuilding()
    {
        // Set the nodes under the building to not walkable
        SetNodesUnwalkable(currentBuilding.transform.position);

        // Clear property blocks to restore original appearance
        if (buildingRenderers != null)
        {
            foreach (Renderer renderer in buildingRenderers)
            {
                renderer.SetPropertyBlock(null);
            }
        }

        // Increment the count for this specific building type
        IncrementBuildingCount(buildingPrefab);

        int currentCount = GetPlacedBuildingCount(buildingPrefab);
        int maxAllowed = GetMaxBuildingCount(buildingPrefab);
        Debug.Log($"Building '{buildingPrefab.name}' placed at {currentBuilding.transform.position}. Buildings of this type placed: {currentCount}/{maxAllowed}. Unwalkable area: {currentUnwalkableWidth}x{currentUnwalkableHeight}");

        currentBuilding = null;
        buildingRenderers = null;
        originalMaterials = null;
        canPlace = false;
    }

    void CancelPlacement()
    {
        if (currentBuilding != null)
        {
            Destroy(currentBuilding);
            currentBuilding = null;
            buildingRenderers = null;
            originalMaterials = null;
            canPlace = false;
        }
    }

    void SetBuildingColor(Color color)
    {
        if (buildingRenderers == null) return;

        foreach (Renderer renderer in buildingRenderers)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

            // Set color using property block instead of modifying material directly
            propBlock.SetColor("_Color", color);
            if (renderer.material.HasProperty("_BaseColor"))
            {
                propBlock.SetColor("_BaseColor", color);
            }

            renderer.SetPropertyBlock(propBlock);
        }
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
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Collider Works");
    }
}