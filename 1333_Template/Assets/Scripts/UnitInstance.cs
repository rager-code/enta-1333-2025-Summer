using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnitInstance : UnitBase
{
    [Header("Component References")]
    [SerializeField] private Animator animator; // for future animation use
    [SerializeField] private SkinnedMeshRenderer unitSkin; // for visual selection
    [SerializeField] private ParticleSystem hurtParticles; // hit feedback
    [SerializeField] public VisualTargetPath vPath; // draws the path visually ----was visual path

    private GridManager gridManager;
    protected AStarPathfinding pathfinder;

    protected List<GridNode> currentPath = new(); // stores the active path
    protected int pathIndex = 0; // current node index in the path

    private bool isMoving = false; // flag to track if unit is currently moving
    public UnitState state;

    public bool IsMoving => isMoving;
    public List<GridNode> CurrentPath => currentPath;

    //private void Start()


    //UnitManager.Instance.allUnitsList.Add(this);
    // Debug.Log($"{name} registered to UnitSelectionManager.");


    public void Initialize(AStarPathfinding pathfinder, UnitType unitType, GridManager grid, VisualTargetPath pathFinderVis)
    {
        // assign dependencies at runtime from army manager
        this.pathfinder = pathfinder;
        base.unitType = unitType;
        gridManager = grid;
        this.vPath = pathFinderVis;
    }

    public override void MoveTo(GridNode targetNode)
    {

        StartCoroutine(vPath.GeneratePathTo(targetNode, this));






        // validate input
        if (pathfinder == null || targetNode == null)
        {
            Debug.LogWarning($"{name} can't move: missing pathfinder or target node.");
            return;
        }

        // find the start node from current world position
        GridNode startNode = gridManager.GetNodeFromWorldPosition(transform.position);


        // generate A* path for the uunit to follow
        int unitWidth = unitType.Width;    // example - ensure these exist
        int unitHeight = unitType.Height;

        currentPath = pathfinder.FindPath(gridManager, startNode, targetNode, unitWidth, unitHeight);
        Debug.Log($"{name} path found with {currentPath.Count} nodes.");

        // draws out the current path
        if (vPath != null)
        {
            vPath.DrawPath(currentPath);
        }
        else
        {
            Debug.LogWarning($"{name} has no pathFinderVisulization assigned!");
        }

        // if path is valid, start moving
        if (currentPath.Count > 0)
        {
            StartPathMovement(currentPath);
            state = UnitState.Moving;
        }
        else
        {
            Debug.LogWarning($"{name} could not find path to target.");
        }

    }

    public void StartPathMovement(List<GridNode> path)
    {
        // reset movement state
        currentPath = path;
        pathIndex = 0;
        isMoving = true;

        Debug.Log($"{name} is beginning movement along path.");
    }

    public void StopMoving()
    {
        // stop all movement and clear path
        isMoving = false;
        pathIndex = 0;

        // clear the current path
        if (currentPath != null)
        {
            currentPath.Clear();
        }

        // clear visual path if it exists
        if (vPath != null)
        {
            vPath.DrawPath(new List<GridNode>());
        }

        // set state back to idle
        state = UnitState.Idle;

        Debug.Log($"{name} movement stopped.");
    }

    public override void DoMove()
    {
        // check if we're still allowed to move
        if (!isMoving || currentPath == null || pathIndex >= currentPath.Count)
        {
            isMoving = false;
            return;
        }

        // move toward the current target node
        Vector3 target = currentPath[pathIndex].WorldPosition + Vector3.up * 0.5f;
        transform.position = Vector3.MoveTowards(transform.position, target, unitType.MoveSpeed * Time.deltaTime);

        // advance to next node if close enough
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            pathIndex++;
            Debug.Log($"{name} reached waypoint {pathIndex}/{currentPath.Count}");
        }

        // stop when done
        if (pathIndex >= currentPath.Count)
        {
            isMoving = false;
            state = UnitState.Idle;
            Debug.Log($"{name} reached destination.");
        }
    }

    public override void PerTick()
    {
        // called by the RTS update manager
        if (state == UnitState.Moving)
            DoMove();
    }

    public void Select()
    {
        // highlight on selection (optional)
        // unitSkin.material.color = Color.cyan;
        Debug.Log($"{name} selected.");
    }

    public void Deselect()
    {
        // remove selection highlight (optional)
        // unitSkin.material.color = Color.white;
        Debug.Log($"{name} deselected.");
    }
}