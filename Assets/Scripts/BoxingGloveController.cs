using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloveController : MonoBehaviour, IPulsable, IPickupable
{
    private bool pickedUp = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void pulse()
    {
        if(pickedUp)
        {
            return;
        }
        Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position, .2f);
        if (hit)
        {
            PlayerController playerController = hit.gameObject.GetComponent<PlayerController>();
            if (playerController != null && playerController.currentItem != Item.BoxingGlove)
            {
                playerController.currentItem = Item.BoxingGlove;
                playerController.currentItemController = this;
                playerController.lastMove.currentItem = Item.BoxingGlove;
                playerController.lastMove.currentItemController = this;
                GetComponent<Renderer>().enabled = false;
                pickedUp = true;
            }
        }
    }

    public int pulsePriority()
    {
        return 2;
    }

    public bool canPulseDeadPlayer()
    {
        return false;
    }
    public void undoPickup()
    {
        GetComponent<Renderer>().enabled = true;
        pickedUp = false;
    }
}
