using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombotController : MonoBehaviour, IPulsable, IUndoable, IBurnable
{

    private bool exploded;
    private List<GameObject> explosions = new List<GameObject>();
    public Sprite explosionSprite;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void shift(Vector3 shiftPoint)
    {
        transform.position = shiftPoint;
    }

    public void deleteExplosions()
    {
        foreach (GameObject explosion in explosions)
        {
            GameObject.Destroy(explosion);
        }
        explosions.Clear();
    }
    IEnumerator waitToDeleteExplosions()
    {
        yield return new WaitForSeconds(0.2f);

        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        if (!playerController.isDying)
        {
            deleteExplosions();
        }

    }

    public void drawExplosion(Vector3 location)
    {
        GameObject explosion = new GameObject();
        explosion.transform.position = location;
        explosion.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = explosion.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "GridObject";
        sr.sprite = explosionSprite;
        explosions.Add(explosion);
    }

    public void explode()
    {
        // Should really move the undo state off of the player controller and into some game manager code
        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        GetComponent<Renderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        exploded = true;
        if(playerController.moveChain.Count > 0)
        {
            MoveRecord playerMoveRecord = playerController.moveChain.Pop();
            playerMoveRecord.toMove.enemyMoves.Add(new EnemyMove(MoveType.Explode, MoveDirection.Up, this)); // It "blew up" ... get it?
            playerController.moveChain.Push(playerMoveRecord);
        }


        foreach (Vector3 i in Global.adjacentVectors)
        {
            drawExplosion(transform.position + i);
            Collider2D objectCollider = (Physics2D.OverlapCircle(transform.position + i, .2f));
            if (objectCollider != null)
            {

                IBurnable burnableObject = objectCollider.gameObject.GetComponent<IBurnable>();
                if (burnableObject != null)
                {
                    burnableObject.burn();
                }
            }
        }

        StartCoroutine(waitToDeleteExplosions());
    }

    private bool checkDirection(MoveDirection direction)
    {
        Vector3 checkVector = Global.moveVectors[direction];

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkVector, Mathf.Infinity, ~LayerMask.GetMask("FloorObject"));
        PlayerController playerController = hit.collider.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (Physics2D.OverlapCircle(transform.position + checkVector, .2f) == null)
            {
                Debug.Log("Move Bombot.");
                shift(transform.position + checkVector);

                if(playerController.moveChain.Count > 0)
                {
                    MoveRecord playerMoveRecord = playerController.moveChain.Pop();
                    playerMoveRecord.toMove.enemyMoves.Add(new EnemyMove(MoveType.Move, direction, this));
                    playerController.moveChain.Push(playerMoveRecord);
                }

                if (Physics2D.OverlapCircle(transform.position + checkVector, .2f) != null)
                {
                    explode();
                }
            }
            else
            {
                explode();
            }
            return true;
        }
        return false;
    }

    public void pulse()
    {
        Debug.Log("Bombot pulse.");
        if (exploded)
        {
            return;
        }
        foreach (MoveDirection i in Enum.GetValues(typeof(MoveDirection)))
        {
            if (checkDirection(i))
            {
                break;
            }
        }
    }

    public int pulsePriority()
    {
        return 10;
    }

    public bool canPulseDeadPlayer()
    {
        return true;
    }

    public void undo(MoveType moveType, MoveDirection moveDirection)
    {
        Debug.Log("Undo bombot.");
        Debug.Log(transform.position);
        if (moveType == MoveType.Move)
        {
            shift(transform.position - Global.moveVectors[moveDirection]);
        }
        else if(moveType == MoveType.Explode)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            exploded = false;
            deleteExplosions();
        }
        Debug.Log(transform.position);
    }

    public void burn()
    {
        explode();
    }
}
