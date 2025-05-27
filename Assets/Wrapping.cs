using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Wrapping : MonoBehaviour
{
    private bool cooldown = false;
    public bool isDuplicate = false;
    public float ghostLifetimeBuffer = 0.02f; // little extra to ensure it arrives

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Wall") || cooldown || isDuplicate)
            return;

        cooldown = true;
        StartCoroutine(ResetCooldown());

        // 1) Snapshot current movement
        var gm = GetComponent<GridMovement>();
        Vector3 oldPos    = transform.position;
        Vector3 oldDir    = gm.moveDirection;
        Vector3 oldTarget = oldPos + oldDir;

        // 2) Spawn a ghost that will move oldPos → oldTarget, then self‑destroy
        var ghost = Instantiate(gameObject, oldPos, transform.rotation);
        ghost.name = gameObject.name + "_ghost";

        // mark it as duplicate so it skips wrap logic
        var wrapDup = ghost.GetComponent<Wrapping>();
        wrapDup.isDuplicate = true;
        wrapDup.enabled     = false;  // disable this Wrapping on the ghost

        // disable the snake mover on the ghost
        var gmDup = ghost.GetComponent<GridMovement>();
        gmDup.enabled = false;

        // start the ghost‐movement coroutine on the original Wrapping script
        StartCoroutine(GhostMoveAndDie(ghost.transform, oldPos, oldTarget, gm.speed));

        // 3) Teleport THIS snake head as before
        gm.wrapBufferedDirection = gm.bufferedDirection;
        gm.bufferedDirection     = gm.moveDirection;
        gm.hasWrappedStep        = true;

        Vector3 newPos  = transform.position;
        Vector3 wallPos = collision.transform.position;

        if (Mathf.Abs(wallPos.x - 16.5f) < 0.01f)      newPos.x = -16.5f;
        else if (Mathf.Abs(wallPos.x + 16.5f) < 0.01f) newPos.x =  16.5f;

        if (Mathf.Abs(wallPos.y - 8.5f) < 0.01f)       newPos.y = -8.5f;
        else if (Mathf.Abs(wallPos.y + 8.5f) < 0.01f)  newPos.y =  8.5f;

        newPos = GridMovement.SnapToGrid(newPos);
        transform.position = newPos;

        gm.isMoving  = false;
        gm.origPos   = newPos;
        gm.targetPos = newPos;
    }

    private IEnumerator GhostMoveAndDie(Transform ghostT, Vector3 from, Vector3 to, float speed)
    {
        float totalDist = Vector3.Distance(from, to);
        float moved = 0f;
        while (moved < totalDist)
        {
            float step = speed * Time.deltaTime;
            ghostT.position = Vector3.MoveTowards(ghostT.position, to, step);
            moved += step;
            yield return null;
        }

        // ensure exact
        ghostT.position = to;
        yield return new WaitForSeconds(ghostLifetimeBuffer);
        Destroy(ghostT.gameObject);
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        cooldown = false;
    }
}
