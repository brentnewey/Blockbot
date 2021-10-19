using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public Transform pushPoint;

    void Start()
    {
        pushPoint.parent = null;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, pushPoint.position, moveSpeed * Time.deltaTime);
    }

    public void push(Vector3 proposedMove)
    {
        pushPoint.position += proposedMove;
    }

    public void shift(Vector2 shiftPoint)
    {
        transform.position = shiftPoint;
        pushPoint.position = shiftPoint;
    }

    public bool canPush(Vector2 proposedMove)
    {
        return (Physics2D.OverlapCircle((Vector2)pushPoint.position + proposedMove, .2f) == null);
    }
}
