using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayerState : PlayerStateItem {
    private StateItem tempNextState;
    public override void CheckSwitchState() {
        tempNextState = null;
        if (Unit.InCombat) {
            tempNextState = PlayerController.CombatState;
        }
        if (PlayerController.IsInventoryMenuOpen) {
            tempNextState = PlayerController.MenuState;
        }
        if (PlayerController.IsInteracting) {

            tempNextState = PlayerController.InteractingState;

        }
        
        SwitchState(tempNextState);
    }

    public override void UpdateState() {
        CheckSwitchState();
    }

    public override void EnterState() {
        //Debug.Log("entering baseplayer ");
        if (CurState != null) {
           // Debug.Log("entering baseplayer ");
        }
    }

    public override void ExitState() {
       // Debug.Log("exiting baseplayer");
    }

    public override void InitSubState() {
        if (PlayerController.Move == new Vector2(0, 0) && !PlayerController.InCombat
            && !PlayerController.IsInventoryMenuOpen && tempNextState != PlayerController.InteractingState) {
            PlayerController.IdleState.SetUpParentState(this);
            SetUpChildState(PlayerController.IdleState);
        }
        else if (PlayerController.Move != new Vector2(0, 0) && !PlayerController.InCombat
           && !PlayerController.IsInventoryMenuOpen && tempNextState != PlayerController.InteractingState) {
            PlayerController.IdleState.SetUpParentState(this);
            SetUpChildState(PlayerController.WalkState);
        }
    }
}


