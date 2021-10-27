using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUndoable
{
    void undo(MoveType moveType, MoveDirection moveDirection);

}
