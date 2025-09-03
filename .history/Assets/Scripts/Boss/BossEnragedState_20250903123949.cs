using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnragedState : IBossState
{
    public void Enter(Boss boss)
    {
        Debug.Log("Boss vào Phase 1");
    
    }

    public void Update(Boss boss)
    {
        // logic hành vi Phase 1
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Boss rời Phase 1");
    }
}
