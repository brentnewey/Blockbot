using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IBurnable
{

    public float moveSpeed = 5f;
    public Transform movePoint;

    public LayerMask whatStopsMovement;
    public Sprite punchSprite;

    private bool hasPulsed = false;
    public bool isDying = false;
    private bool hasTurned = false;
    private MoveDirection facing = MoveDirection.Down;
    public Item currentItem = Item.None;
    public IPickupable currentItemController = null;

    public MoveUnit lastMove = new MoveUnit(MoveType.Face, MoveDirection.Down, Item.None, null);
    public Stack<MoveRecord> moveChain = new Stack<MoveRecord>();


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
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void pushMove(MoveUnit move)
    {
        moveChain.Push(new MoveRecord(lastMove, move));
        lastMove = move;
    }

    void pushFacingMove(MoveType moveType)
    {
        pushMove(new MoveUnit(moveType, facing, currentItem, currentItemController));
    }

    void undoMove()
    {
        if (moveChain.Count == 0)
        {
            return;
        }
        MoveRecord moveRecordToUndo = moveChain.Pop();
        lastMove = moveRecordToUndo.fromMove;

        if (moveRecordToUndo.toMove.moveType == MoveType.Face)
        {
            facing = moveRecordToUndo.fromMove.moveDirection;
            transform.eulerAngles = Global.faceVectors[moveRecordToUndo.fromMove.moveDirection];
        }
        if (moveRecordToUndo.toMove.moveType == MoveType.Push)
        {
            Vector3 blockLocationVector = transform.position + Global.moveVectors[moveRecordToUndo.toMove.moveDirection];
            Collider2D blockCollider = Physics2D.OverlapCircle(blockLocationVector, .2f);
            blockCollider.gameObject.GetComponent<BlockController>().shift(transform.position);
        }

        if (moveRecordToUndo.toMove.moveType == MoveType.Move || moveRecordToUndo.toMove.moveType == MoveType.Push)
        {
            transform.position -= Global.moveVectors[moveRecordToUndo.toMove.moveDirection];
            movePoint.position = transform.position;

            if (moveRecordToUndo.fromMove.currentItem != moveRecordToUndo.toMove.currentItem)
            {
                currentItem = moveRecordToUndo.fromMove.currentItem;
                currentItemController = moveRecordToUndo.fromMove.currentItemController;
                moveRecordToUndo.toMove.currentItemController.undoPickup();
            }

        }

        if (moveRecordToUndo.toMove.moveType == MoveType.Punch)
        {
            Vector3 blockShiftVector = transform.position + Global.moveVectors[moveRecordToUndo.toMove.moveDirection];
            Vector3 blockLocationVector = blockShiftVector + Global.moveVectors[moveRecordToUndo.toMove.moveDirection];

            Collider2D blockCollider = Physics2D.OverlapCircle(blockLocationVector, .2f);
            blockCollider.gameObject.GetComponent<BlockController>().shift(blockShiftVector);
            currentItem = Item.BoxingGlove;
        }

        isDying = false;

        foreach(EnemyMove enemyMove in moveRecordToUndo.toMove.enemyMoves)
        {
            enemyMove.undoableController.undo(enemyMove.moveType, enemyMove.moveDirection);
        }

        hasPulsed = true;
    }

    void Update()
    {
        // Block all input when game is paused
        if (PauseMenuController.IsGamePaused)
        {
            return;
        }

        // Ensure PauseMenuController exists (will auto-create if needed)
        var pauseController = PauseMenuController.Instance;

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);


        if (Vector3.Distance(transform.position, movePoint.position) <= float.Epsilon)
        {
            if (!hasPulsed)
            {
                executePulse();
            }

            if (Input.GetKeyDown(KeyCode.R))
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

            // Prevent a whole class of bugs by just not allowing this state
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1 && Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentItem == Item.BoxingGlove)
                {
                    Vector3 proposedPunch = Global.moveVectors[facing];
                    Collider2D punchCollider = Physics2D.OverlapCircle(movePoint.position + proposedPunch, .2f);
                    bool drawPunch = false;

                    if (punchCollider)
                    {
                        BlockController blockController = punchCollider.gameObject.GetComponent<BlockController>();
                        if (blockController != null)
                        {
                            if (blockController.canPush(proposedPunch))
                            {
                                drawPunch = true;
                                blockController.shift(punchCollider.gameObject.transform.position + proposedPunch);
                                currentItem = Item.None;
                                pushFacingMove(MoveType.Punch);
                            }
                            else
                            {
                                return;
                            }
                        }

                    }
                    else
                    {
                        drawPunch = true;
                    }
                    if (drawPunch)
                    {
                        GameObject punch = new GameObject();
                        punch.transform.position = movePoint.position + proposedPunch;
                        punch.AddComponent<SpriteRenderer>();
                        SpriteRenderer sr = punch.GetComponent<SpriteRenderer>();
                        sr.sortingLayerName = "GridObject";
                        sr.sprite = punchSprite;
                        GameObject.Destroy(punch, 0.2f);
                    }
                }
            }

            // Check facing change
            bool hasChanged = false;
            if (Input.GetAxisRaw("Horizontal") == 1f && facing != MoveDirection.Right)
            {
                facing = MoveDirection.Right;
                hasChanged = true;
            }
            else if (Input.GetAxisRaw("Horizontal") == -1f && facing != MoveDirection.Left)
            {
                facing = MoveDirection.Left;
                hasChanged = true;
            }
            else if (Input.GetAxisRaw("Vertical") == 1f && facing != MoveDirection.Up)
            {
                facing = MoveDirection.Up;
                hasChanged = true;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f && facing != MoveDirection.Down)
            {
                facing = MoveDirection.Down;
                hasChanged = true;
            }

            if(hasChanged)
            {
                transform.eulerAngles = Global.faceVectors[facing];
                hasTurned = true;
                pushFacingMove(MoveType.Face);
                StartCoroutine(waitToMove());
            }

            if (hasTurned && (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)))
            {
                hasTurned = false;
            }

            // Don't allow movement after a facing change for a short period of time, which is set by coroutine or if the player raises the key
            if (hasTurned)
            {
                return;
            }

            Vector3 proposedMove;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                proposedMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0f);
            }
            else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                proposedMove = new Vector3(0f, Input.GetAxisRaw("Vertical"));
            }
            else
            {
                return;
            }
            if (Physics2D.OverlapCircle(movePoint.position + proposedMove, .2f, whatStopsMovement))
            {
                return;
            }

            bool isPush = false;
            Collider2D blockCollider = Physics2D.OverlapCircle(movePoint.position + proposedMove, .2f);

            if (blockCollider)
            {
                BlockController blockController = blockCollider.gameObject.GetComponent<BlockController>();
                if (blockController != null)
                {
                    if (blockController.canPush(proposedMove))
                    {
                        blockCollider.gameObject.GetComponent<BlockController>().push(proposedMove);
                        isPush = true;
                    }
                    else
                    {
                        return;
                    }
                }

            }
            pushFacingMove(isPush ? MoveType.Push : MoveType.Move);
            movePoint.position += proposedMove;
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
            if (!isDying || pulsable.canPulseDeadPlayer())
            {
                pulsable.pulse();
            }
        }
        hasPulsed = true;
    }

    public void burn()
    {
        Debug.Log("Burn player.");
        isDying = true;
    }
}
