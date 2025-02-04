using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainerUIController : MonoBehaviour
{
    [SerializeField] protected bool isOpen;
    [SerializeField] protected AudioClip dropCLip;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected Unit curUnit;

    public List<InventoryItem> initialItems = new List<InventoryItem>();
    public virtual void ActivateUI(bool isActive) { }
    public virtual void PrepareInventory() { }



    /*---------------------------------------------------------------------
    |  Method PrepareUI()
    |
    |  Purpose: Sets up UI toaccept draggs,swaps, and selecting items
    |
    |   Parameters: none
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
    public virtual void PrepareUI() {
    }



    protected virtual void HandleDescriptionClear(int obj) {
    }

    protected virtual void HandleItemActionRequest(int curItemIndex) {
    }

    public virtual void PreformAction(int curItemIndex) {
    }

    /*---------------------------------------------------------------------
    |  Method HandleDragging(int curItemIndex)
    |
    |  Purpose: Handles dragging item when clicking on inventory item
    |
    |   Parameters: int curItemIndex = index of item being dragged
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void HandleDragging(int curItemIndex) {
    }

    /*---------------------------------------------------------------------
    |  Method HandleSwapItems(int curitemIndex, int itemIndexSwap)
    |
    |  Purpose: Handles swaping items between two slots
    |
    |   Parameters: int curitemIndex = index of item being dragged
    |               int itemIndexSwap = index of item being hovered over
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void HandleSwapItems(int curitemIndex, int itemIndexSwap) { }

    /*---------------------------------------------------------------------
    |  Method HandleSwapItems(int curitemIndex, int itemIndexSwap)
    |
    |  Purpose: Handles when showing description when clicking on item
    |
    |   Parameters: int curitemIndex = index of item being selected
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void HandleDescriptionRequest(int curItemIndex) { }

    protected virtual string PrepareDescription(InventoryItem inventoryItem) {
        String newDescription = "";
        newDescription += inventoryItem.item.description + "\n\n";
        // Debug.Log("--->"+inventoryItem.itemState.Count);
        for (int i = 0; i < inventoryItem.itemState.Count; i++) {
            //  Debug.Log("--<" + i);
            newDescription += inventoryItem.itemState[i].itemParameter.ParameterName.ToString() + ": " +
                inventoryItem.itemState[i].val.ToString() + "/" + inventoryItem.itemState[i].val.ToString() + "\n";
        }
        newDescription += "Weight: " + inventoryItem.item.Weight + "\n" +
            "Value: " + inventoryItem.item.Value;
        return newDescription;
    }

    public bool IsOpen { get { return isOpen; } }
}
