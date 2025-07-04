using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Targeting : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;   // Movement speed in units per second

    private Coroutine moveRoutine;

    public void StartMoving(List<GridNode> path)
    {
        //Debug.Log("StartingMoving------");
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveAlongPath(path));
    }

    public void StopMoving()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        // Reset velocity/animation here if you want
    }

    private IEnumerator MoveAlongPath(List<GridNode> path)
    {
        foreach (GridNode node in path)
        {
            Vector3 target = node.WorldPosition + Vector3.up * 0.5f;   // Target position slightly above ground

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        moveRoutine = null; // path completed
    }
}