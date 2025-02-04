using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;

namespace Inventory.UI {
    public class LootPageUI : ItemContainerUI {
        public event Action<int,LootPageUI> onItemActionRequest, onStartDrag, onDescriptionRequest, onDescriptionClear, onEndDrag;
        public event Action<int, int,LootPageUI> onSwapItems;
        public event Action<InventorySO, int> onTransferItems;
        public void Start() {
            Hide();
        }
    
        public void InitalizeItems(List<InventoryItemUI> listToAddTo, InventoryItemUI curItem) {
            if (curItem != null) {
                itemList.Add(curItem);
                // handles what happens if itemUI is left clicked
                curItem.onItemClicked += HandleShowItemActions;
                // handles what happens if itemUI being dragged
                curItem.onItemBeginDrag += HandleBeginDrag;
                // handesl what happens if itemUI has another itemUI dropped on it
                curItem.onItemDroppedOn += HandleSwap;
                // handles what happens when item is dropped after being dragged.
                curItem.onItemEndDrag += HandleEndDrg;
                // handles what happens if  itemUI right clicked
                curItem.onRightMouseBtnClick += HandleShowItemActions;
                curItem.onPointerEnter += HandlePointerEnter;
                curItem.onPointerExit += HandlePointerExit;
        

            }
        }


        /*---------------------------------------------------------------------
        |  Method CreateInventoryUI(int inventorySize)
        |
        |  Purpose: Creates itemList by filling the list of inventoryUI
        |           with "inventorySeize" elements
        |  
        |   Parameters: int inventorySize = size of listoftiems 
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public void CreateInventoryUI(InventorySO curDataOfInventory) {
            // creates invneotry based on the size of curDataOfInvenotry
            // or of size 9 if daata is null
            dataOfInventory = curDataOfInventory;
            int childCount = 9;
            if (dataOfInventory != null) {
                childCount = dataOfInventory.GetSize();
            }
            for (int i = 0; i < childCount; i++) {
                CreateItemUI();
            }

        }
        protected void CreateItemUI() {
            InventoryItemUI curItem = Instantiate(item);
            curItem.transform.SetParent(container);
            curItem.gameObject.transform.localScale = Vector3.one;
            InitalizeItems(itemList, curItem);
            curItem.ItemContainer = this;

        }

    /*---------------------------------------------------------------------
    |  Method Show()
    |
    |  Purpose: Shows the game object from view
    |
    |   Parameters: none
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
        public virtual void Show() {
            gameObject.SetActive(true);
            ResetSelected();
        }

        /*---------------------------------------------------------------------
        |  Method Show()
        |
        |  Purpose: hides the game object from view
        |  
        |   Parameters: none
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public virtual void Hide() {
            itemMenu.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();

        }

        protected override void HandlePointerExit(InventoryItemUI currentItem) {
            //  Debug.Log(" << pointer exit "+ currentItem.name);
            int index = itemList.IndexOf(currentItem);

            if (index == -1) {
                return;
            }
            currentItem.Deselect();
            if (onDescriptionClear != null && !isDragging) {
                DeselectAllItems2();
                onDescriptionClear.Invoke(index , this);
            }

        }

        protected override void HandlePointerEnter(InventoryItemUI currentItem) {
            //   Debug.Log(" >> pointer enter "+ currentItem.name);
            int index = itemList.IndexOf(currentItem);

            if (index == -1) {
                return;
            }

            currentItem.Select();
            // onDescriptionRequest?.Invoke(index);
            if (onDescriptionRequest != null && !isDragging) {
                onDescriptionRequest.Invoke(index, this);
            }
        }

        /*---------------------------------------------------------------------
        |  Method HandleBeginDrag(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being dragged to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being dragged
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        protected override void HandleBeginDrag(InventoryItemUI currentItem) {
            // grabs index of clicked on slot
            int index = itemList.IndexOf(currentItem);
            
            currentItemIndex = index;
            // selects current slot
            HandleItemSelection(currentItem);
            if (onStartDrag != null) {

                onStartDrag.Invoke(index, this);
            }

        }

        /*---------------------------------------------------------------------
        |  Method HandleSwap(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being swapped to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being swapped
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        protected override void HandleSwap(InventoryItemUI currentItem) {

            int index = itemList.IndexOf(currentItem);
            if (index <= -1 || currentItem == null || index >= itemList.Count) {
                return;
            }
            // InventoryItemUI itemToSwap = new InventoryItemUI();
            if (currentItemIndex >= 0) {
                InventoryItemUI itemToSwap = itemList[currentItemIndex];
                // swaps items
                if (onSwapItems != null) {
                    onSwapItems.Invoke(currentItemIndex, index,this);
                }
            }
            // if item dragged from another lootPageUI
            else {
                if (onTransferItems != null) {
                    onTransferItems.Invoke(dataOfInventory, index);
                }
            }
            // selects currenly dragged item
            HandleItemSelection(currentItem);
        }

        /*---------------------------------------------------------------------
        |  Method HandleEndDrg(InventoryItemUI currentItem)
        |
        |  Purpose: Clears the draggedItem when mouse is let go
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being let go
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        protected override void HandleEndDrg(InventoryItemUI currentItem) {
            ResetDraggedItem();
            if (onEndDrag != null) {
                onEndDrag.Invoke(0,this);
            }
            //mouseFollower.Clear();
        }
        protected override void HandleShowItemActions(InventoryItemUI currentItem) {
            int index = itemList.IndexOf(currentItem);

            //check if slot tring to preform is in bounds
            if (index <= -1) {
                return;
            }
            onItemActionRequest?.Invoke(index, this);

        }

 
        
    }


}
