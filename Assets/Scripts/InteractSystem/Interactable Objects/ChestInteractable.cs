using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteractable : Interactable {
    // private bool isActive=false;
    [SerializeField] InventorySO inventopry;
    protected override void Interact(GameObject curInteractWith) {

        Activate();
        IsActive = !IsActive;
        PlayerController player = curInteractWith.GetComponent<PlayerController>();
        if (player != null) {
            player.LootUIController.AssignInventoryData(inventopry);
            player.LootUIController.AssignOwner(this.transform.position);
            player.LootUIController.ActivateUI(true);
        }
        //Debug.Log("talking "+IsActive);
        // inventoryController.ActivateUI(IsActive);

    }

    public override void Deactivate() {
        
        isActive = false;
        if (interactedWith != null) {
            PlayerController player = interactedWith.GetComponent<PlayerController>();
            if (player != null) {
                player.LootUIController.ActivateUI(false);
            }
        }
        interactedWith = null;
    }


}