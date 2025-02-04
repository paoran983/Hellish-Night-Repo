using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateItem {
    
    private bool isComplete;

    private Unit unit;

    private Controller controller;

    private StateItem curState;

    private StateItem childState = null;

    private StateItem parentState = null;

    public abstract void EnterState();

    public abstract void ExitState();

    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }

    public abstract void CheckSwitchState();

    public virtual void InitSubState() { }

    public virtual void UpdateAllStates() {
        UpdateState();
        if(childState != null) {
            childState.UpdateAllStates();
        }
    }
    public virtual void FixedUpdateAllStates() {
        FixedUpdateState();
        if (childState != null) {
            childState.FixedUpdateAllStates();
        }
    }

    public virtual void ExitAllStates() {
        ExitState();
        if (childState != null) {
            childState.ExitAllStates();
        }
    }

    public virtual void SetUpChildState(StateItem newChild) {
        childState = newChild;
        if (newChild != null) {
            newChild.ParentState = this;
        }
    }

    public virtual void SetUpParentState(StateItem newParent) {
        parentState = newParent;
    }

    public virtual void Initialize(Unit unit,Controller controller) {
        this.unit = unit;
        isComplete = false;
        this.controller = controller;
        this.curState = controller.CurState;
    }

    public virtual void Refresh() {
        isComplete = false;
    }

    public virtual void SwitchState(StateItem nextState) {
        StateItem oldState = CurState;
        // switches states if new state entered
        if (nextState != CurState) {
            CurState = nextState;
            oldState.ExitAllStates();
            CurState.Refresh();
            CurState.EnterState();
        }
    }

    public bool IsComplete{ get { return isComplete; } set { isComplete = value; } }
    public Unit Unit {  get { return unit;  } set { unit = value; }}
    public Controller Controller { get { return controller; } }
    public StateItem CurState { get { return curState; } set { curState = value; } }
    public StateItem ParentState { get { return parentState; } set {  parentState = value; } }
    public StateItem ChildState { get { return childState; } set {  childState = value; } }


}
