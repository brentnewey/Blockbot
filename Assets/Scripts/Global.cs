using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
public static class Global
{

    public static Dictionary<MoveDirection, Vector3> faceVectors = new Dictionary<MoveDirection, Vector3>(){
        {MoveDirection.Down, new Vector3(0.0f, 0.0f, 0.0f)},
        {MoveDirection.Up, new Vector3(0.0f, 0.0f, 180.0f)},
        {MoveDirection.Left, new Vector3(0.0f, 0.0f, -90.0f)},
        {MoveDirection.Right, new Vector3(0.0f, 0.0f, 90.0f)},
    };
    public static Dictionary<MoveDirection, Vector3> moveVectors = new Dictionary<MoveDirection, Vector3>(){
        {MoveDirection.Left, Vector3.left},
        {MoveDirection.Right, Vector3.right},
        {MoveDirection.Up, Vector3.up},
        {MoveDirection.Down, Vector3.down},
    };
}
