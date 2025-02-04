using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNpcState : NpcStateItem
{
    private StateItem tempNextState;
    public override void CheckSwitchState() {
        tempNextState = null;
        if (Unit != null) {
            if (Unit.InCombat) {
                tempNextState = npcController.CombatState;
            }
        }
        else {
        }
        SwitchState(tempNextState);
    }

    public override void UpdateState() {
        CheckSwitchState();
    }

    public override void EnterState() {
        if (CurState != null) {
            // Debug.Log("entering baseplayer ");
        }
    }

    public override void ExitState() {
        // Debug.Log("exiting baseplayer");
    }

    public override void InitSubState() {
        SetUpChildState(npcController.IdleState);
    }
}
