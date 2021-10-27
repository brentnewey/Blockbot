using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloveController : MonoBehaviour, IPulsable, IPickupable, IBurnable, IUndoable
{
    private bool pickedUp = false;
    private bool burned = false;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void pulse()
    {
        if(pickedUp || burned)
        {
            return;
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position, .2f);

        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                PlayerController playerController = hit.gameObject.GetComponent<PlayerController>();
                if (playerController != null && playerController.currentItem != Item.BoxingGlove)
                {
                    playerController.currentItem = Item.BoxingGlove;
                    playerController.currentItemController = this;
                    // Should move this stuff to the enemy moves undo system
                    playerController.lastMove.currentItem = Item.BoxingGlove;
                    playerController.lastMove.currentItemController = this;
                    GetComponent<Renderer>().enabled = false;
                    GetComponent<BoxCollider2D>().enabled = false;
                    pickedUp = true;
                }
            }
        }
    }

    public int pulsePriority()
    {
        return 30;
    }

    public bool canPulseDeadPlayer()
    {
        return false;
    }
    public void undoPickup()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        pickedUp = false;
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

    public void undo(MoveType moveType, MoveDirection moveDirection)
    {
        if(moveType == MoveType.Burn)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            burned = false;
        }
    }
}
