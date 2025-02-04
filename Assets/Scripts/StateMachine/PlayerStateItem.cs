using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerStateItem : StateItem
{
    private PlayerController playerController;
    public void InitializePlayerState(Unit unit, PlayerController controller) {
        this.Unit = unit;
        this.playerController = controller;
        this.CurState = controller.CurState;
        InitSubState();
    }
    public override void SwitchState(StateItem nextState) {
        StateItem oldState = CurState;
        if (nextState == null || PlayerController.CurState == null) {
            return;
        }
        // Debug.Log("swithcing states "+nextState.ToString()+"  "+CurState.ToString()+"  "+ PlayerController.CurState.ToString());
        // switches states if new state entered
        if (PlayerController.CurState != nextState && nextState != null) {
            //Debug.Log("swithcing states");
            CurState = nextState;
            PlayerController.CurState.ExitAllStates();
            if (this.ParentState != null) {
                this.ParentState.SetUpChildState(CurState);
            }
            CurState.Refresh();
            CurState.EnterState();
            
        }
        if (CurState.ParentState == null) {
            playerController.CurState = CurState;
            Unit.CurState = CurState;
        }
    }
    public virtual PlayerController PlayerController { get { return playerController; } set { playerController = value; } }
}
