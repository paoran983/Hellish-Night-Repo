using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InMenuPlayerState : PlayerStateItem {
    public override void EnterState() {
        //controller.InventoryMenu();

        PlayerController.ActivateInventoryMemnu();
        Debug.Log("entering InMenuState");
    }

    public override void ExitState() {
        //controller.InventoryMenu();
        
        PlayerController.DeactivateInventoryMemnu();
        Debug.Log("leaving InMenuState");
    }

    public override void UpdateState() {
        CheckSwitchState();
    }

    public override void CheckSwitchState() {
        if(!PlayerController.IsInventoryMenuOpen) {
            SwitchState(PlayerController.BasePlayerState);
        }
    }

    public override void InitSubState() {
    }



}
