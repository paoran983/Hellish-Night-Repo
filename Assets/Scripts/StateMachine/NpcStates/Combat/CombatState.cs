using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : StateItem {
    public override void EnterState() {
       // Debug.Log("entering CombatState");
    }

    public override void ExitState() {
        //Debug.Log("leaving CombatState");
    }

    public override void UpdateState() {
        //Debug.Log("doing CombatState "+Controller.gameObject.name);
        CheckSwitchState();
    }

    public override void CheckSwitchState() {
        if (Unit.InCombat == false) {
            SwitchState(Controller.BaseState);
        }
    }

    public override void InitSubState() {
    }
}
