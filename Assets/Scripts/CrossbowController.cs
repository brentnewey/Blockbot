using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowController : MonoBehaviour, IPulsable, IUndoable, IBurnable
{
    public MoveDirection direction;
    public Material lineMaterial;

    private GameObject killLine;
    private bool burned = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    void DrawKillLine(Vector3 start, Vector3 end, Color color, bool autoErase)
    {
        killLine = new GameObject();
        killLine.transform.position = start;
        killLine.AddComponent<LineRenderer>();
        LineRenderer lr = killLine.GetComponent<LineRenderer>();
        lr.sortingLayerName = "GridObject";
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if(autoErase)
        {
            GameObject.Destroy(killLine, 0.2f);
        }
    }

    void EraseKillLine()
    {
        GameObject.Destroy(killLine);
        killLine = null;
    }

    void Update()
    {

    }

    public void pulse()
    {

        if (burned)
        {
            return;
        }
        Vector3 checkVector = Global.moveVectors[direction];
        Vector3 lineStartVector = transform.position + Global.moveVectors[direction] * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkVector, Mathf.Infinity, ~LayerMask.GetMask("FloorObject"));

        PlayerController playerController = hit.collider.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            DrawKillLine(lineStartVector, hit.collider.transform.position, Color.red, false);
            MoveRecord playerMoveRecord = playerController.moveChain.Pop();
            playerMoveRecord.toMove.enemyMoves.Add(new EnemyMove(MoveType.Kill, direction, this));
            playerController.moveChain.Push(playerMoveRecord);
            playerController.killWithCrossbow();
        }

        BombotController bombotController = hit.collider.gameObject.GetComponent<BombotController>();
        if (bombotController != null)
        {
            DrawKillLine(lineStartVector, hit.collider.transform.position, Color.red, true);
            bombotController.explode();
        }
    }

    public int pulsePriority()
    { 
        return 20;
    }

    public bool canPulseDeadPlayer()
    {
        return true;
    }

    public void undo(MoveType moveType, MoveDirection moveDirection)
    {
        if(moveType == MoveType.Kill)
        {
            EraseKillLine();
        }
        else if (moveType == MoveType.Burn)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            burned = false;
        }
    }

    public void burn()
    {
        // Should really move the undo state off of the player controller and into some game manager code
        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        GetComponent<Renderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        burned = true;
        MoveRecord playerMoveRecord = playerController.moveChain.Pop();
        playerMoveRecord.toMove.enemyMoves.Add(new EnemyMove(MoveType.Burn, MoveDirection.Up, this)); // It "burned up" ... get it?
        playerController.moveChain.Push(playerMoveRecord);
    }
}
