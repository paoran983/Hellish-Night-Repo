using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NpcStateItem : StateItem {
    protected NpcController npcController;
    public void InitializeNpcState(Unit unit, NpcController controller) {
        this.Unit = unit;
        this.npcController = controller;
        this.CurState = controller.CurState;
        InitSubState();
    }
    public override void SwitchState(StateItem nextState) {
        StateItem oldState = CurState;

        if (npcController.CurState == null || nextState == null) {
            return;
        }
        // Debug.Log("swithcing states " + nextState.ToString() + "  " + CurState.ToString() + "  for " + Unit.gameObject.name);
        // switches states if new state entered
        if (npcController.CurState != nextState && nextState != null) {
            //Debug.Log("swithcing states");
            CurState = nextState;
            npcController.CurState.ExitAllStates();
            if (this.ParentState != null) {
                this.ParentState.SetUpChildState(CurState);
            }
            CurState.Refresh();
            CurState.EnterState();

        }
        if (CurState.ParentState == null) {
            npcController.CurState = CurState;
            Unit.CurState = CurState;
        }
    }
    public virtual NpcController NpcController { get { return npcController; } set { npcController = value; } }

}
