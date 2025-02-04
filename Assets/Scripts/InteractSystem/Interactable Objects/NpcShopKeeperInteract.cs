using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcShopKeeperInteract : Interactable {
    // private bool isActive=false;
    [SerializeField] InventorySO inventory;
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private Unit unit;
    private GameObject curInteractWith;
    private bool isStoreOpen;
    protected override void Interact(GameObject curInteractWith) {
        this.curInteractWith = curInteractWith;

        if ( unit != null) {
            dialougeManager.onActivateStore += HandleActivateStore;
            dialougeManager.EnterDialougeMode(unit.DialogeManager.DeafultText);
        }


       // Activate();
        

    }

    private void HandleActivateStore() {
        
        Unit curUnit = GetComponent<Unit>();
        
        
        if (curUnit != null) {
            inventory = curUnit.Inventory;
        }
        
        if (inventory != null) {
            IsActive = !IsActive;
            PlayerController player = curInteractWith.GetComponent<PlayerController>();
            if (player != null) {
                interactedWith = player.gameObject;
                player.ShopPageUIController.AssignSellerUnit(curUnit);
                player.ShopPageUIController.AssignOwner(this.transform.position);
                player.ShopPageUIController.ActivateUI(true);
                Debug.Log("interacting shop");
                isStoreOpen = true;
                dialougeManager.StartCoroutine("ExitDialougeMode");
            }
        }
    }

    public override void Deactivate() {
        dialougeManager.StartCoroutine("ExitDialougeMode");
        isActive = false;
        if (interactedWith != null) {
            PlayerController player = interactedWith.GetComponent<PlayerController>();
            if (player != null) {
                player.ShopPageUIController.ActivateUI(false);
            }
        }
        interactedWith = null;
    }


}