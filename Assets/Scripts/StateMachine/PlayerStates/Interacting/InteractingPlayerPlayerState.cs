using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractingPlayerState : PlayerStateItem {
    private bool debug = false;
    public override void EnterState() {
        Log("entering interctState");
        PlayerController.MovePlayer(new Vector2(0,0));
        Interactable curInteract = PlayerController.PlayerInteract.CheckForInteractable();
        PlayerController.PlayerInteract.StartInteraction();
    }

    public override void ExitState() {
        PlayerController.PlayerInteract.Deactivate();
        Log("leaving interctState");
        PlayerController.IsInteracting = false;
    }

    public override void UpdateState() {
        // if player p[resses take all button while interacting
        if (PlayerController.TakeAllLoot) {
            PlayerController.LootUIController.HandleTakeAllLoot();
        }
        CheckSwitchState();
    }

    public override void CheckSwitchState() {

        if (PlayerController.IsInteracting == false || PlayerController.DialougeManager.IsDoneTalking) {
            Log("done interact");
            PlayerController.IsInteracting = false;
            SwitchState(PlayerController.BasePlayerState);
        }
    }
    public void Log(string str) {
        if (debug) {
            Debug.Log(str);
        }
    }
    public override void InitSubState() {
    }
}
