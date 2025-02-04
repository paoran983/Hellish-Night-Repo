using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkPlayerState : PlayerStateItem {


    public override void EnterState() {
       // PlayerController.MovePlayer(PlayerController.Move);
       // Debug.Log("entering walkState");

    }

    public override void ExitState() {

        PlayerController.MovePlayer(new Vector2(0,0));
        //Debug.Log("leaving walkState");
    }

    public override void UpdateState() {

        CheckSwitchState();
    }
    public override void FixedUpdateState() {
       // Debug.Log(" walking");
        PlayerController.MovePlayer(PlayerController.Move);
    }

    public override void CheckSwitchState() {
        StateItem tempNextState = null;
        if (PlayerController.Move == new Vector2(0, 0) && !PlayerController.InCombat
             && !PlayerController.IsInventoryMenuOpen && PlayerController.CurState != PlayerController.InteractingState) {
            tempNextState = PlayerController.IdleState;
        }
        SwitchState(tempNextState);
    }

    public override void InitSubState() {
    }
}
