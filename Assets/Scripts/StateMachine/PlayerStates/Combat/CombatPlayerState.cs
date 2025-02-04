using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatPlayerState : PlayerStateItem
{
    StateItem tempState;
    public override void EnterState() {
        //Debug.Log("entering CombatState");
    }

    public override void ExitState() {
        //Debug.Log("leaving CombatState");
    }

    public override void UpdateState() {
        // Debug.Log("doing CombatState " + Controller.gameObject.name);
        CheckSwitchState();
    }

    public override void CheckSwitchState() {
        tempState = null;
        if (Unit.InCombat == false) {
            tempState = PlayerController.BaseState;
        }
        if (PlayerController.IsInventoryMenuOpen) {
            tempState = PlayerController.MenuState;
        }
        SwitchState(tempState);
    }

    public override void InitSubState() {
    }
}
