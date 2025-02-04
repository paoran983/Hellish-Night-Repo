using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory {
    public class LootingUIController : ItemContainerUIController {
        [SerializeField] private LootPageUI inventoryUI;
        [SerializeField] private LootPageUI playerInventoryUI;
        [SerializeField] private int inventorySize;
        [SerializeField] private int playerInventorySize;
        [SerializeField] private InventorySO inventoryData;
        [SerializeField] private InventorySO playerInvenotry;
        [SerializeField] private Interactable NpcInteractable;
        [SerializeField] private float offsetFloat;
        [SerializeField] private Canvas canvas;
        [SerializeField] private InventoryItemUI curSelectedItem;
        [SerializeField] private InventorySO curInventory;
        [SerializeField] private int curSelectedIndex;
        [SerializeField] private PlayerController player;
        [SerializeField] private BasicButtonUI takeAllLootButton;
        [SerializeField] private UnuseableItemSO money;
        public void Awake() {
            // creates inventory  
            PrepareInventory(inventoryData);
            inventoryData.onInventoryUpdated += UpdateInventoryUI;
            PrepareInventory(playerInvenotry);
            playerInvenotry.onInventoryUpdated += UpdatePlayerInventoryUI;
            // prepares UI
            PrepareUI(inventoryUI, inventoryData);
            PrepareUI(playerInventoryUI, playerInvenotry);
            // player = GetComponent<PlayerController>();
            PrepareUIButton();

        }
        public void Start() {
            ActivateUI(false);
        }

        public override void ActivateUI(bool isActive) {

            if (isActive) {
                inventoryUI.Show();
                playerInventoryUI.Show();
                takeAllLootButton.gameObject.SetActive(true);
                //updates UI to match data
                inventoryUI.ResetAllItems();
                foreach (var item in inventoryData.GetCurrentInventoryState()) {
                    inventoryUI.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count);
                }
                playerInventoryUI.ResetAllItems();
                foreach (var item in playerInvenotry.GetCurrentInventoryState()) {
                    playerInventoryUI.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count);
                }

                isOpen = true;
            }
            else {
                inventoryUI.Hide();
                playerInventoryUI.Hide();
                takeAllLootButton.gameObject.SetActive(false);
                isOpen = false;
            }

        }
        public void PrepareInventory(InventorySO curinventory) {

            foreach (InventoryItem item in initialItems) {
                if (item.IsEmpty()) {
                    continue;
                }
                curinventory.AddItem(item);
            }
        }

        /*---------------------------------------------------------------------
        |  Method UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
        |
        |  Purpose: Updates the UI when data is changed in the model
        |
        |   Parameters: Dictionary<int, InventoryItem> inventorySate dict that 
        |               represents the current data in the inventory model
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate) {
            // resets previous information
            inventoryUI.ResetAllItems();
            // updates UI with current inventory data
            foreach (var item in inventorySate) {
                //    Debug.Log("---ww " + item.Value.count);
                inventoryUI.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count);
            }
        }
        /*---------------------------------------------------------------------
        |  Method UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
        |
        |  Purpose: Updates the UI when data is changed in the model
        |
        |   Parameters: Dictionary<int, InventoryItem> inventorySate dict that 
        |               represents the current data in the inventory model
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void UpdatePlayerInventoryUI(Dictionary<int, InventoryItem> inventorySate) {
            //   Debug.Log("***** updateing");
            // resets previous information
            if (isOpen) {
                playerInventoryUI.ResetAllItems();
                // updates UI with current inventory data
                foreach (var item in inventorySate) {
                    //    Debug.Log("---ww " + item.Value.count);
                    playerInventoryUI.UpdateData(item.Key,
                        item.Value.item.GetImage(),
                        item.Value.count);
                }
            }
        }

        public void HandleTakeAllLoot() {
            Dictionary<int, InventoryItem> inventorySate = inventoryData.GetCurrentInventoryState();
            foreach (var item in inventorySate) {
                playerInvenotry.AddItem(item.Value);
                inventoryData.RemoveItem(item.Key, item.Value.count);
            }
        }
        /*---------------------------------------------------------------------
        |  Method PrepareUI()
        |
        |  Purpose: Sets up UI toaccept draggs,swaps, and selecting items
        |
        |   Parameters: none
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        public void PrepareUI(LootPageUI curInventoryUI, InventorySO curInventoryData) {
            curInventoryUI.CreateInventoryUI(curInventoryData);
            curInventoryUI.onDescriptionRequest += HandleDescriptionRequest;
            curInventoryUI.onSwapItems += HandleSwapItems;
            curInventoryUI.onStartDrag += HandleDragging;
            curInventoryUI.onItemActionRequest += HandleItemActionRequest;
            curInventoryUI.onTransferItems += HandleTransferItems;
            curInventoryUI.onDescriptionClear += HandleDescriptionClear;
            curInventoryUI.onEndDrag += HandleEndDrag;



        }
        public void PrepareUIButton() {
            takeAllLootButton.InitalizeButton("TAKE ALL");
            takeAllLootButton.onButtonClicked += HandleTakeAllButtonPress;
            takeAllLootButton.onButtonReleased += HandleButtonReleased;
            takeAllLootButton.onPointerEnter += HandelSelectButton;
            takeAllLootButton.onPointerExit += HandelDeselectButton;

        }
        private void HandleTakeAllButtonPress(ButtonUI button) {
            HandleTakeAllLoot();
            button.Press();
        }

        private void HandelSelectButton(ButtonUI button) {
            button.Select();
        }

        private void HandelDeselectButton(ButtonUI button) {
            button.Deselct();
        }

        private void HandleButtonReleased(ButtonUI button) {
            button.Release();
        }

        private void HandleEndDrag(int arg1, LootPageUI uI) {
            curInventory = null;
            curSelectedIndex = -1;
        }

        public void PreparePlayerUI() {
            // playerInventoryUI.CreateInventoryUI(inventorySize);
            // playerInventoryUI.onDescriptionRequest += HandlePlayerDescriptionRequest;
            playerInventoryUI.onSwapItems += HandleSwapItems;
            playerInventoryUI.onStartDrag += HandleDragging;
            playerInventoryUI.onItemActionRequest += HandleItemActionRequest;
            playerInventoryUI.onTransferItems += HandleTransferItems;
            playerInventoryUI.onDescriptionClear += HandleDescriptionClear;


        }
        private void HandleDescriptionClear(int obj, LootPageUI curPageUI) {
            if (curInventory == null && curSelectedIndex == -1) {
                //inventoryUI.ResetSelectedLeaveMenu();
                curPageUI.ResetDescription();
            }
            //curInventory = null;
            // curSelectedIndex = -1;
        }
        private void HandleTransferItems(InventorySO itemToTansterSO, int transferIndex) {
            if (curInventory != null) {

                // Debug.Log("swapping " + curSelectedIndex + " from " + curInventory.name + " with " + transferIndex + " from " + itemToTansterSO.name);
                InventoryItem tempItem = curInventory.GetItemAt(curSelectedIndex);
                InventoryItem itemToTransfer = itemToTansterSO.GetItemAt(transferIndex);
                if (tempItem.item != null && itemToTransfer.item == tempItem.item) {
                    if (tempItem.item.isStackable && itemToTransfer.item.isStackable) {

                        itemToTansterSO.IncreaseItemCount(transferIndex, tempItem.count + itemToTransfer.count);
                        curInventory.RemoveItemByItem(tempItem, tempItem.count);
                        return;
                    }
                }
                if (curInventory == playerInvenotry) {

                    if (tempItem.item == money) {
                        player.Unit.Stats.Money -= tempItem.count;
                    }
                }
                else if (itemToTansterSO == playerInvenotry) {
                    if (tempItem.item == money) {
                        player.Unit.Stats.Money += tempItem.count;
                    }
                }

                curInventory.SetItemAt(curSelectedIndex, itemToTransfer);
                itemToTansterSO.SetItemAt(transferIndex, tempItem);
                //curInventory.RemoveItem(curSelectedIndex,tempItem.count);

            }

        }

        private void HandleItemActionRequest(int curItemIndex, LootPageUI curPageUI) {
            InventorySO curInventory = curPageUI.DataOfInventory;
            InventoryItem tempItemSelected = curInventory.GetItemAt(curItemIndex);
            // if player is moveing money item
            if (curInventory == playerInvenotry) {

                if (tempItemSelected.item == money) {
                    player.Unit.Stats.Money -= tempItemSelected.count;
                }
            }
            else {
                if (tempItemSelected.item == money) {
                    player.Unit.Stats.Money += tempItemSelected.count;
                }
            }

            if (curInventory == playerInvenotry) {

                inventoryData.AddItem(tempItemSelected.item, tempItemSelected.count, tempItemSelected.itemState, tempItemSelected.moveListState, tempItemSelected.level);
                playerInvenotry.RemoveItem(curItemIndex, tempItemSelected.count);


            }
            // if player is buying item
            else if (player != null) {

                playerInvenotry.AddItem(tempItemSelected.item, tempItemSelected.count, tempItemSelected.itemState, tempItemSelected.moveListState, tempItemSelected.level);
                inventoryData.RemoveItem(curItemIndex, tempItemSelected.count);

            }
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
        private void HandleDragging(int curItemIndex, LootPageUI curPageUI) {
            InventoryItem inventoryItem = curPageUI.DataOfInventory.GetItemAt(curItemIndex);
            // avoids empty slots
            if (inventoryItem.IsEmpty()) {
                // Debug.Log("tttest");
                return;
            }

            curInventory = curPageUI.DataOfInventory;
            curSelectedIndex = curItemIndex;
            // creates dragged item 
            curPageUI.CreateDraggedItem(inventoryItem.item.GetImage(), inventoryItem.count);
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
        private void HandleSwapItems(int curitemIndex, int itemIndexSwap, LootPageUI curPageUI) {
            curPageUI.DataOfInventory.SwapItems(curitemIndex, itemIndexSwap);

        }


        /*---------------------------------------------------------------------
        |  Method HandleSwapItems(int curitemIndex, int itemIndexSwap)
        |
        |  Purpose: Handles when showing description when clicking on item
        |
        |   Parameters: int curitemIndex = index of item being selected
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void HandleDescriptionRequest(int curItemIndex, LootPageUI curPageUI) {
            //Debug.Log("Request descript");
            if (curItemIndex < 0 || curItemIndex >= curPageUI.DataOfInventory.GetSize() || (curInventory != null && curItemIndex != -1)) {
                return;
            }
            InventoryItem curInventoryItem = curPageUI.DataOfInventory.GetItemAt(curItemIndex);
            // avoids empty slots

            if (curInventoryItem.IsEmpty()) {
                //curPageUI.ResetDescription();
                return;
                // return;
            }
            // updates description page
            ItemSO curItem = curInventoryItem.item;
            curPageUI.UpdateDescription(curItemIndex, curItem.GetImage(),
                curItem.GetName(), PrepareDescription(curInventoryItem));

        }
        /*---------------------------------------------------------------------
        |  Method HandleSwapItems(int curitemIndex, int itemIndexSwap)
        |
        |  Purpose: Handles when showing description when clicking on item
        |
        |   Parameters: int curitemIndex = index of item being selected
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void HandlePlayerDescriptionRequest(int curItemIndex) {
            if (curItemIndex < 0 || curItemIndex >= playerInvenotry.GetSize()) {
                return;
            }
            InventoryItem curInventoryItem = playerInvenotry.GetItemAt(curItemIndex);
            // avoids empty slots
            if (curInventoryItem.IsEmpty()) {
                playerInventoryUI.ResetDescription();
                return;
                // return;
            }
            // updates description page
            ItemSO curItem = curInventoryItem.item;
            playerInventoryUI.UpdateDescription(curItemIndex, curItem.GetImage(),
                curItem.GetName(), PrepareDescription(curInventoryItem));

        }


        private void DropItem(int curItemIndex, int count) {
            inventoryData.RemoveItem(curItemIndex, count);
            inventoryUI.ResetSelected();
            audioSource.PlayOneShot(dropCLip);
        }


        public override void PreformAction(int curItemIndex) {
            InventoryItem inventoryItem = inventoryData.GetItemAt(curItemIndex);
            // avoids empty slots
            if (inventoryItem.IsEmpty()) {
                return;
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null) {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);
                audioSource.PlayOneShot(itemAction.actionSFX);
                if (inventoryData.GetItemAt(curItemIndex).IsEmpty()) {
                    inventoryUI.ResetSelected();
                }
            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null) {
                // Debug.Log("remvoing action");
                inventoryData.RemoveItem(curItemIndex, 1);
            }
        }

        public void AssignInventoryData(InventorySO newInventory) {
            if (newInventory == null) {
                Debug.Log("given null inventory");
                return;
            }
            if (inventoryData != null) {
                inventoryData.onInventoryUpdated -= UpdateInventoryUI;
            }
            inventoryData = newInventory;
            inventoryData.onInventoryUpdated += UpdateInventoryUI;
            inventoryUI.DataOfInventory = newInventory;
            inventoryUI.ResetAllItems();

            // updates UI with current inventory data
            foreach (var item in newInventory.GetCurrentInventoryState()) {
                //    Debug.Log("---ww " + item.Value.kcount);
                inventoryUI.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count);
            }
        }

        public void AssignOwner(Vector3 newOwnerPos) {
            // inventoryUI.transform.position = new Vector3(newOwnerPos.x, newOwnerPos.y+offsetFloat, newOwnerPos.z);
            inventoryUI.transform.position = canvas.transform.TransformPoint(new Vector3(newOwnerPos.x, newOwnerPos.y + offsetFloat, newOwnerPos.z));
        }

        public PlayerController Player { get { return player; } set { player = value; } }

    }
}
