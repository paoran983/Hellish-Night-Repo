using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleNpcState : NpcStateItem
{
    public override void EnterState() {
        //PlayerController.MovePlayer(new Vector2(0, 0));
        // Debug.Log("entering IdleState");
    }

    public override void ExitState() {
        // Debug.Log("leaving IdleState");
    }

    public override void UpdateState() {
        CheckSwitchState();
    }

    public override void CheckSwitchState() {

    }
}
