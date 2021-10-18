using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public Transform pushPoint;

    // Start is called before the first frame update
    void Start()
    {
        pushPoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, pushPoint.position, moveSpeed * Time.deltaTime);
    }

    public void push(Vector2 proposedMove)
    {
        pushPoint.position += (Vector3)proposedMove;
    }

    public void shift(Vector2 shiftPoint)
    {
        Debug.Log(shiftPoint);
        transform.position = shiftPoint;
        pushPoint.position = shiftPoint;
    }

    public bool canPush(Vector2 proposedMove)
    {
        Debug.Log("Can push?");
        Debug.Log(Physics2D.OverlapCircle((Vector2)pushPoint.position + proposedMove, .2f));
        return (Physics2D.OverlapCircle((Vector2)pushPoint.position + proposedMove, .2f) == null);
    }
}
