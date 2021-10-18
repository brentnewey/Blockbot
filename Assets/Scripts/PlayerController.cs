using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Item
{
    BoxingGlove,
    None
}

public enum MoveType
{
    Face,
    Move,
    Push,
    Punch
}

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}

public class MoveUnit
{
    public MoveType moveType;
    public MoveDirection moveDirection;
    public Item currentItem;
    public IPickupable currentItemController;

    public MoveUnit(MoveType moveType, MoveDirection moveDirection, Item currentItem, IPickupable currentItemController)
    {
        this.moveType = moveType;
        this.moveDirection = moveDirection;
        this.currentItem = currentItem;
        this.currentItemController = currentItemController;
    }
}

class MoveRecord
{
    public MoveUnit fromMove;
    public MoveUnit toMove;

    public MoveRecord(MoveUnit fromMove, MoveUnit toMove)
    {
        this.fromMove = fromMove;
        this.toMove = toMove;
    }
}

class PulsableComparer : IComparer<IPulsable>
{
    public int Compare(IPulsable p1, IPulsable p2)
    {
        return p1.pulsePriority().CompareTo(p2.pulsePriority());
    }
}

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public Transform movePoint;

    public LayerMask whatStopsMovement;
    public string currentScene;

    private bool hasPulsed = false;
    private bool isDying = false;
    private bool hasTurned = false;
    private MoveDirection facing = MoveDirection.Down;
    public Item currentItem = Item.None;
    public IPickupable currentItemController = null;

    public MoveUnit lastMove = new MoveUnit(MoveType.Face, MoveDirection.Down, Item.None, null);
    Stack<MoveRecord> moveChain = new Stack<MoveRecord>();

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null; 
    }



    IEnumerator waitToMove()
    {
        yield return new WaitForSeconds(0.1f);
        hasTurned = false;
    }

    IEnumerator waitToReset()
    {
        yield return new WaitForSeconds(1f);
        if(Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(currentScene);
        }
    }

    void pushMove(MoveUnit move)
    {
        moveChain.Push(new MoveRecord(lastMove, move));
        lastMove = move;
    }

    void undoMove()
    {
        if (moveChain.Count == 0)
        {
            return;
        }
        MoveRecord moveRecordToUndo = moveChain.Pop();
        Debug.Log("Undo Move:");
        Debug.Log(moveRecordToUndo.fromMove);
        Debug.Log(moveRecordToUndo.toMove);
        lastMove = moveRecordToUndo.fromMove;
        if (moveRecordToUndo.toMove.moveType == MoveType.Face)
        {
            if(moveRecordToUndo.fromMove.moveDirection == MoveDirection.Down)
            {
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                facing = MoveDirection.Down;
            }
            else if (moveRecordToUndo.fromMove.moveDirection == MoveDirection.Up)
            {
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                facing = MoveDirection.Up;
            }
            else if (moveRecordToUndo.fromMove.moveDirection == MoveDirection.Left)
            {
                transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                facing = MoveDirection.Left;
            }
            else if (moveRecordToUndo.fromMove.moveDirection == MoveDirection.Right)
            {
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                facing = MoveDirection.Right;
            }
        }
        if (moveRecordToUndo.toMove.moveType == MoveType.Push)
        {
            Vector3 blockLocationVector;

            if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Left)
            {
                blockLocationVector = new Vector3(transform.position.x - 1, transform.position.y, 0);
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Right)
            {
                blockLocationVector = new Vector3(transform.position.x + 1, transform.position.y, 0);
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Up)
            {
                blockLocationVector = new Vector3(transform.position.x, transform.position.y + 1, 0);
            }
            else
            {
                blockLocationVector = new Vector3(transform.position.x, transform.position.y - 1, 0);
            }

            Collider2D collider = Physics2D.OverlapCircle(blockLocationVector, .2f);
            collider.gameObject.GetComponent<BlockController>().shift(transform.position);
        }

        if (moveRecordToUndo.toMove.moveType == MoveType.Move || moveRecordToUndo.toMove.moveType == MoveType.Push)
        {
            if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Left)
            {
                transform.position = new Vector3(transform.position.x + 1, transform.position.y, 0);
                movePoint.position = transform.position;
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Right)
            {
                transform.position = new Vector3(transform.position.x - 1, transform.position.y, 0);
                movePoint.position = transform.position;
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Up)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - 1, 0);
                movePoint.position = transform.position;
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Down)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 1, 0);
                movePoint.position = transform.position;
            }

            Debug.Log(moveRecordToUndo.toMove.currentItem);
            Debug.Log(moveRecordToUndo.fromMove.currentItem);
            if (moveRecordToUndo.fromMove.currentItem != moveRecordToUndo.toMove.currentItem)
            {
                currentItem = moveRecordToUndo.fromMove.currentItem;
                currentItemController = moveRecordToUndo.fromMove.currentItemController;
                moveRecordToUndo.toMove.currentItemController.undoPickup();
            }

        }

        if (moveRecordToUndo.toMove.moveType == MoveType.Punch)
        {
            Vector3 blockLocationVector;
            Vector3 blockShiftVector;

            if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Left)
            {
                blockLocationVector = new Vector3(transform.position.x - 2, transform.position.y, 0);
                blockShiftVector = new Vector3(transform.position.x - 1, transform.position.y, 0);
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Right)
            {
                blockLocationVector = new Vector3(transform.position.x + 2, transform.position.y, 0);
                blockShiftVector = new Vector3(transform.position.x + 1, transform.position.y, 0);
            }
            else if (moveRecordToUndo.toMove.moveDirection == MoveDirection.Up)
            {
                blockLocationVector = new Vector3(transform.position.x, transform.position.y + 2, 0);
                blockShiftVector = new Vector3(transform.position.x, transform.position.y + 1, 0);
            }
            else if(moveRecordToUndo.toMove.moveDirection == MoveDirection.Down)
            {
                blockLocationVector = new Vector3(transform.position.x, transform.position.y - 2, 0);
                blockShiftVector = new Vector3(transform.position.x, transform.position.y - 1, 0);
            }
            else
            {
                throw new Exception();
            }

            Collider2D collider = Physics2D.OverlapCircle(blockLocationVector, .2f);
            collider.gameObject.GetComponent<BlockController>().shift(blockShiftVector);
            currentItem = Item.BoxingGlove;
        }

        isDying = false;
        hasPulsed = false;
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);


        if (Vector2.Distance(transform.position, movePoint.position) <= float.Epsilon)
        {
            if(!hasPulsed)
            {
                executePulse();
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(waitToReset());
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                undoMove();
            }

            if (isDying)
            {
                return;
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(currentItem == Item.BoxingGlove)
                {
                    Vector3 proposedPunch;
                    if(facing == MoveDirection.Right)
                    {
                        proposedPunch = new Vector3(1, 0);
                    }
                    else if(facing == MoveDirection.Left)
                    {
                        proposedPunch = new Vector3(-1, 0);
                    }
                    else if (facing == MoveDirection.Up)
                    {
                        proposedPunch = new Vector3(0, 1);
                    }
                    else if (facing == MoveDirection.Down)
                    {
                        proposedPunch = new Vector3(0, -1);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    Collider2D punchCollider = Physics2D.OverlapCircle(movePoint.position + proposedPunch, .2f);

                    if (punchCollider)
                    {
                        BlockController blockController = punchCollider.gameObject.GetComponent<BlockController>();
                        if (blockController != null)
                        {
                            if (blockController.canPush(proposedPunch))
                            {
                                Debug.Log("Attempting to shift.");
                                blockController.shift(punchCollider.gameObject.transform.position + proposedPunch);
                                currentItem = Item.None;
                                pushMove(new MoveUnit(MoveType.Punch, facing, currentItem, currentItemController));
                            }
                            else
                            {
                                Debug.Log("Block stop");
                                return;
                            }
                        }

                    }
                }
            }

            // Check facing change
            if (Input.GetAxisRaw("Horizontal") == 1f && facing != MoveDirection.Right)
            {
                facing = MoveDirection.Right;
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                hasTurned = true;
                pushMove(new MoveUnit(MoveType.Face, MoveDirection.Right, currentItem, currentItemController));
                StartCoroutine(waitToMove());
            }
            if (Input.GetAxisRaw("Horizontal") == -1f && facing != MoveDirection.Left)
            {
                facing = MoveDirection.Left;
                transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                hasTurned = true;
                pushMove(new MoveUnit(MoveType.Face, MoveDirection.Left, currentItem, currentItemController));
                StartCoroutine(waitToMove());
            }
            if (Input.GetAxisRaw("Vertical") == 1f && facing != MoveDirection.Up)
            {
                facing = MoveDirection.Up;
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                hasTurned = true;
                pushMove(new MoveUnit(MoveType.Face, MoveDirection.Up, currentItem, currentItemController));
                StartCoroutine(waitToMove());
            }
            if (Input.GetAxisRaw("Vertical") == -1f && facing != MoveDirection.Down)
            {
                facing = MoveDirection.Down;
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                hasTurned = true;
                pushMove(new MoveUnit(MoveType.Face, MoveDirection.Down, currentItem, currentItemController));
                StartCoroutine(waitToMove());
            }

            if (hasTurned && (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)))
            {
                hasTurned = false;
            }

            if (hasTurned)
            {
                return;
            }

            Vector2 proposedMove;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                proposedMove = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
            }
            else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                proposedMove = new Vector2(0f, Input.GetAxisRaw("Vertical"));
            }
            else
            {
                return;
            }
            if (Physics2D.OverlapCircle((Vector2)movePoint.position + proposedMove, .2f, whatStopsMovement))
            {
                Debug.Log("Wall stop");
                return;
            }

            bool isPush = false;
            Collider2D collider = Physics2D.OverlapCircle((Vector2)movePoint.position + proposedMove, .2f);

            if(collider)
            {
                BlockController blockController = collider.gameObject.GetComponent<BlockController>();
                if(blockController != null)
                {
                    if (blockController.canPush(proposedMove))
                    {
                        collider.gameObject.GetComponent<BlockController>().push(proposedMove);
                        isPush = true;
                    }
                    else
                    {
                        Debug.Log("Block stop");
                        return;
                    }
                }

            }

            if (Input.GetAxisRaw("Horizontal") == 1f)
            {
                if(isPush)
                {
                    pushMove(new MoveUnit(MoveType.Push, MoveDirection.Right, currentItem, currentItemController));
                }
                else
                {
                    pushMove(new MoveUnit(MoveType.Move, MoveDirection.Right, currentItem, currentItemController));
                }
            }
            else if (Input.GetAxisRaw("Horizontal") == -1f)
            {
                if (isPush)
                {
                    pushMove(new MoveUnit(MoveType.Push, MoveDirection.Left, currentItem, currentItemController));
                }
                else
                {
                    pushMove(new MoveUnit(MoveType.Move, MoveDirection.Left, currentItem, currentItemController));
                }
            }
            else if (Input.GetAxisRaw("Vertical") == 1f)
            {
                if (isPush)
                {
                    pushMove(new MoveUnit(MoveType.Push, MoveDirection.Up, currentItem, currentItemController));
                }
                else
                {
                    pushMove(new MoveUnit(MoveType.Move, MoveDirection.Up, currentItem, currentItemController));
                }
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                if (isPush)
                {
                    pushMove(new MoveUnit(MoveType.Push, MoveDirection.Down, currentItem, currentItemController));
                }
                else
                {
                    pushMove(new MoveUnit(MoveType.Move, MoveDirection.Down, currentItem, currentItemController));
                }
            }

            movePoint.position += (Vector3)proposedMove;
        }
        else
        {
            hasPulsed = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        ExitController exitController = collider.gameObject.GetComponent<ExitController>();
        if (exitController != null)
        {
            exitController.goToExit();
        }

    }

    public void killWithCrossbow()
    {
        isDying = true;
    }

    private void executePulse()
    {
        Debug.Log(currentItem);
        var objects = GameObject.FindGameObjectsWithTag("Object");
        List<IPulsable> pulsableList = new List<IPulsable>();
        foreach (var obj in objects)
        {
            IPulsable pulsableComponent = obj.GetComponent<IPulsable>();
            if (pulsableComponent != null)
            {
                pulsableList.Add(pulsableComponent);
            }
        }
        pulsableList.Sort(new PulsableComparer());
        foreach (IPulsable pulsable in pulsableList)
        {
            if(!isDying || pulsable.canPulseDeadPlayer())
            {
                pulsable.pulse(); 
            }
        }
        hasPulsed = true;
    }
}
