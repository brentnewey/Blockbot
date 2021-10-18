using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPulsable
{
    void pulse();
    int pulsePriority();
    bool canPulseDeadPlayer();

}
