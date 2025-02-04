using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NpcInteract : Interactable
{
   // private bool isActive=false;
    [SerializeField] private LootingUIController inventoryController;
    [SerializeField] private DialougeManager dialougeManager;

    [SerializeField] private TextAsset inkText;
    [SerializeField] private Unit unit;
    protected override void Interact(GameObject interactWith) {

        isActive = !isActive;
        if (isActive == true && unit != null) {
           
            dialougeManager.EnterDialougeMode(unit.DialogeManager.DeafultText);
        }
        else {
            dialougeManager.StartCoroutine("ExitDialougeMode");
        }

    }
    public override void Deactivate() {
        //dialougeManager.StartCoroutine("ExitDialougeMode");
        
    }







}
