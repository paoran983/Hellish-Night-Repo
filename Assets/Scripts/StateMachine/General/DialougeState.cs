using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialougeState : StateItem {
    public override void EnterState() {
        Debug.Log("entering DialougeState");
    }

    public override void ExitState() {
        Debug.Log("leaving DialougeState");
    }

    public override void UpdateState() {
        Debug.Log("doing DialougeState");
    }

    public override void CheckSwitchState() {
        throw new System.NotImplementedException();
    }

    public override void InitSubState() {
        throw new System.NotImplementedException();
    }
}
