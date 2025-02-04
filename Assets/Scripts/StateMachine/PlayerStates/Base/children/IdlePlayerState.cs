using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IdlePlayerState : PlayerStateItem {
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
        StateItem tempNextState = null;

        if (PlayerController.Move != new Vector2(0, 0) ) {
            //Debug.Log("checking idle");
            tempNextState = PlayerController.WalkState;
            SwitchState(tempNextState);
        }
        
        
    }

}