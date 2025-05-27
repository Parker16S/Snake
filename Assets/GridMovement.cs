using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    public bool isMoving;
    public bool allowInput = true;

    public Vector3 moveDirection = Vector3.zero;
    public Vector3 bufferedDirection = Vector3.zero;

    public Vector3 origPos, targetPos;
    public float speed;

    [HideInInspector] public bool hasWrappedStep = false;
    [HideInInspector] public Vector3 wrapBufferedDirection = Vector3.zero;

    // Not Head Stuff



    void Start()
    {
        bufferedDirection = Vector3.right;

        // if its not the snakeHead (Player)
        int index = GameManager.Instance.snakeParts.IndexOf(this.gameObject);
        if (index > 0)
        {
            Debug.Log("Index: " + index);
            Debug.Log("Part: " + gameObject.name);

            allowInput = false;

            // Get the piece ahead of this one in the list
            GameObject pieceAhead = GameManager.Instance.snakeParts[index - 1];
            GridMovement gmAhead = pieceAhead.GetComponent<GridMovement>();

            // Snap to grid just in case, to avoid drift
            origPos = SnapToGrid(transform.position);

            // Copy movement direction from piece ahead
            moveDirection = gmAhead.moveDirection;
            bufferedDirection = gmAhead.moveDirection;

            // Set target based on direction
            targetPos = gmAhead.origPos;

        }
    }

    void Update()
    {

        if (GameManager.Instance.snakeParts.IndexOf(this.gameObject) > 0)
            return;
            
        float step = speed * Time.deltaTime;

        // 1) Read input as before…
        if (allowInput)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && moveDirection != Vector3.down)
                bufferedDirection = Vector3.up;
            if (Input.GetKeyDown(KeyCode.RightArrow) && moveDirection != Vector3.left)
                bufferedDirection = Vector3.right;
            if (Input.GetKeyDown(KeyCode.DownArrow) && moveDirection != Vector3.up)
                bufferedDirection = Vector3.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow) && moveDirection != Vector3.right)
                bufferedDirection = Vector3.left;
        }

        // 2) Pick next grid step when not moving
        if (!isMoving)
        {
            isMoving = true;

            if (hasWrappedStep)
            {
                origPos = SnapToGrid(transform.position);
                targetPos = origPos + moveDirection;
                hasWrappedStep = false;

                bufferedDirection = wrapBufferedDirection;
                wrapBufferedDirection = Vector3.zero;
            }
            else
            {
                moveDirection = bufferedDirection;
                origPos = SnapToGrid(transform.position);
                targetPos = origPos + moveDirection;
            }
        }

        // 3) Smooth move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        if (Vector3.Distance(transform.position, targetPos) < 0.001f)
        {
            transform.position = targetPos;
            isMoving = false;
        }
    }

    // Helpers for grid snapping
    public static float RoundToNearest(float value, float step)
        => Mathf.Round(value / step) * step;

    public static Vector3 SnapToGrid(Vector3 pos)
        => new Vector3(
            RoundToNearest(pos.x, 0.5f),
            RoundToNearest(pos.y, 0.5f),
            0
        );
}
