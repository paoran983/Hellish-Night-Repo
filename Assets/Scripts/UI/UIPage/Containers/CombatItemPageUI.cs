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

namespace Inventory.UI {
    public class CombatItemPageUI : ItemContainerUI {

        //   [SerializeField] private InventoryItemUI pageBackground;
        public event Action<int> onItemActionRequest, onStartDrag, onDescriptionRequest;
        public event Action<int, int> onSwapItems;
        public event Action onDescriptionClear;
        public event Action<InventoryItemUI, InventoryItemUI, int, int> onTransferItems;


        public void InitalizeItems(List<InventoryItemUI> listToAddTo, InventoryItemUI curItem) {
            if (curItem != null) {
                itemList.Add(curItem);
                // handles what happens if itemUI is left clicked
                curItem.onItemClicked += HandleShowItemActions;
                // handles what happens if itemUI being dragged
                curItem.onItemBeginDrag += HandleBeginDrag;
                // handesl what happens if itemUI is being dropped on another itemUI
                curItem.onItemDroppedOn += HandleSwap;
                curItem.onAddingAbility += HandleAddingAbility;
                curItem.onEquipping += HandleEquipping;
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
        public void CreateInventoryUI(int inventorySize) {
            // initlaizes invenotry based on how many cildren the controlPanel gameboject has.
            // this is determined in the unity editor
            // this collects all inventory item slots in the current scene
            // Debug.Log("making " + inventorySize);
            int childCount = inventorySize;

            //  pageBackground.onPointerEnter += HandlePointerEnter;
            //  Debug.Log("making " + curIntenvory.gameObject.name+"  "+childCount);
            for (int i = 0; i < childCount; i++) {

                InventoryItemUI curItem = Instantiate(item);
                curItem.transform.SetParent(container);
                curItem.gameObject.transform.localScale = Vector3.one;
                InitalizeItems(itemList, curItem);

            }
        }

        private void HandleEquipping(InventoryItemUI uI) {
            //throw new NotImplementedException();
        }

        private void HandleAddingAbility(InventoryItemUI uI) {
            // throw new NotImplementedException();
        }

        private void HandlePointerExit(InventoryItemUI currentItem) {
            //  Debug.Log(" << pointer exit "+ currentItem.name);
            int index = itemList.IndexOf(currentItem);

            if (index == -1) {
                return;
            }

            if (onDescriptionClear != null && !isDragging) {
                DeselectAllItems2();
                onDescriptionClear.Invoke();
            }

        }

        private void HandlePointerEnter(InventoryItemUI currentItem) {
              
            int index = itemList.IndexOf(currentItem);
            
            if (index == -1) {
                return;
            }

            // onDescriptionRequest?.Invoke(index);
            if (onDescriptionRequest != null && !isDragging) {
                onDescriptionRequest.Invoke(index);
            }
        }

        private void HandleSwapWithDifInvenotry(InventoryItemUI uI1, InventoryItemUI uI2) {
            //    Debug.Log("trying to swap " + uI1.name + "  " + uI2.name);
        }

        /*---------------------------------------------------------------------
        |  Method  HandleItemSelection(InventoryItemUI currentItem)
        |
        |  Purpose: Sends the inforamtion about the slot 
        |           being selected to inventory controller
        |
        |   Parameters: InventoryItemUI currentItem = the current slot being selected
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void HandleItemSelection(InventoryItemUI currentItem) {
            int index = itemList.IndexOf(currentItem);

            if (index == -1) {
                return;
            }

            /* // onDescriptionRequest?.Invoke(index);
             if (onDescriptionRequest != null) {
                 onDescriptionRequest.Invoke(index);
             }*/
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
        private void HandleBeginDrag(InventoryItemUI currentItem) {

            int index = itemList.IndexOf(currentItem);
            if (index == -1) {
                //mouseFollower.Toggle(false);
                return;
            }
            // grabs index of clicked on slot
            currentItemIndex = index;
            // selects current slot
            HandleItemSelection(currentItem);
            if (onStartDrag != null) {
                onStartDrag.Invoke(index);
            }
            /*
            mouseFollower.Toggle(true);
            mouseFollower.SetData(index==0?image:image2, count);*/
        }

        public void CreateDraggedItem(Sprite sprite, int count) {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, count);
            isDragging = true;
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
        private void HandleSwap(InventoryItemUI currentItem) {

            int index = itemList.IndexOf(currentItem);
            //  Debug.Log("---sddf" + index);
            if (index <= -1 || currentItem == null || index >= itemList.Count) {
                return;
            }
            //    Debug.Log(currentItemIndex);
            // InventoryItemUI itemToSwap = new InventoryItemUI();
            if (currentItemIndex >= 0) {
                InventoryItemUI itemToSwap = itemList[currentItemIndex];

                //Debug.Log("--- swaping " + currentItem.gameObject.name + "  " + itemToSwap.gameObject.name);
                // tried to swap with invalid or out of bounds
                if (currentItem.GetParentInventory() != itemToSwap.GetParentInventory()) {


                    if (onTransferItems != null) {
                        onTransferItems.Invoke(itemToSwap, currentItem, itemToSwap.GetIndex(), currentItem.GetIndex());
                    }
                    return;
                }


                // swaps items
                if (onSwapItems != null) {
                    onSwapItems.Invoke(currentItemIndex, index);
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
        private void HandleEndDrg(InventoryItemUI currentItem) {
            ResetDraggedItem();
            //mouseFollower.Clear();
        }

        private void ResetDraggedItem() {
            mouseFollower.Toggle(false);
            currentItemIndex = -1;
            isDragging = false;
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemCount) {
            // checks if inbounds

            if (itemList.Count > itemIndex) {
                //Debug.Log("++++" + itemIndex);
                // update data
                // Debug.Log("++++" + itemList.Count);
                itemList[itemIndex].SetData(itemImage, itemCount);
            }

        }
        public void UpdateAbilityData(int itemIndex, Sprite itemImage, int itemCount) {
            // checks if inbounds
            // Debug.Log(abilityList.Count+ " ++++ " + itemIndex);


        }
        public void UpdateEquipmentData(int itemIndex, Sprite itemImage, int itemCount) {
            // checks if inbounds
            // Debug.Log(abilityList.Count+ " ++++ " + itemIndex);



        }
        private void HandleShowItemActions(InventoryItemUI currentItem) {
            int index = itemList.IndexOf(currentItem);

            //check if slot tring to preform is in bounds
            if (index <= -1) {
                return;
            }
            onItemActionRequest?.Invoke(index);

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
        public void Show() {
            gameObject.SetActive(true);
            // ensure the descriptiuon page is cleared when player opens inventory
            // itemDescription.ResetDescription();
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
        public void Hide() {
            itemMenu.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();

        }

        public void ResetSelected() {
            //  itemDescription.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actoionName, Action action) {
            itemMenu.AddButon(actoionName, action);
        }
        /*---------------------------------------------------------------------
        *  Method ShowItemAction(int currentItemIndex)
        *
        *  Purpose: shows action menu and moves it to current item clicked.
        *           is called bu inventoryUIController
        *
        *   Parameters: int currentItemIndex = index of current item clicked
        *
        *  Returns: none
        *-------------------------------------------------------------------*/
        public void ShowItemAction(int currentItemIndex) {
            itemMenu.Toggle(true);
            itemMenu.transform.position = itemList[currentItemIndex].transform.position;
        }
        private void DeselectAllItems() {
            // deselects all items 
            foreach (InventoryItemUI item in itemList) {
                item.Deselect();
            }
            // hides item action menu
            itemMenu.Toggle(false);
        }
        private void DeselectAllItems2() {
            // deselects all items 
            foreach (InventoryItemUI item in itemList) {
                item.Deselect();
            }
            // hides item action menu
            //itemMenu.Toggle(false);
        }
        public List<InventoryItemUI> GetItemList() {
            return itemList;
        }



        public Unit CurUnit {
            get {
                return curUnit;
            }
            set {
                curUnit = value;
            }
        }

        public void UpdateDescription(int curItemIndex, Sprite image, string name, string description) {
            //updates desctiption to currently slected item
            // itemDescription.SetDescription(image, name, description);
            mouseFollower.SetDescriptionData(image, name, description);
            mouseFollower.ToggleDescription(true);
            //clears all current selections
            DeselectAllItems();
            // selects the desired slot
            itemList[curItemIndex].Select();
        }

        public void ResetAllItems() {
            foreach (var item in itemList) {
                item.ResetData();
                item.Deselect();
            }
        }
        public void ResetDescription() {
            //  itemDescription.ResetDescription();
            mouseFollower.ToggleDescription(false);
            mouseFollower.Description.ResetDescription();

        }


    }
}
