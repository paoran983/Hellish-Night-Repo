using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopPageUIController : ItemContainerUIController {
    [SerializeField] private LootPageUI inventoryUI;
    [SerializeField] private LootPageUI playerInventoryUI;
    [SerializeField] private int inventorySize;
    [SerializeField] private int playerInventorySize;
    [SerializeField] private InventorySO sellerInventoryData;
    [SerializeField] private InventorySO playerInvenotry;
    [SerializeField] private Interactable NpcInteractable;
    [SerializeField] private float offsetFloat;
    [SerializeField] private Canvas canvas;
    [SerializeField] private InventoryItemUI curSelectedItem;
    [SerializeField] private InventorySO curInventory;
    [SerializeField] private int curSelectedIndex;
    [SerializeField] private PlayerController player;
    [SerializeField] private BasicButtonUI confirmButton;
    [SerializeField] private BasicButtonUI resetButton;
    [SerializeField] private BasicButtonUI cancelButton;
    [SerializeField] private int costOwed;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject transactionTextHolder;
    [SerializeField] private TMP_Text transactionText;
    [SerializeField] private List<InventoryItem> playerOwedItems;
    [SerializeField] private List<InventoryItem> sellerOwedItems;
    [SerializeField] private Unit sellerUnit;
    [SerializeField] private Unit playerUnit;
    [SerializeField] private UnuseableItemSO money;
    private Dictionary<int, InventoryItem> initialPlayerInventory;
    private Dictionary<int, InventoryItem> initialSellerInventory;
    [SerializeField] private float playerOwnedWeight;
    [SerializeField] private float sellerOwnedWeight;
    private int savedOwed;

    public void Awake() {
        // player = GetComponent<PlayerController>();
        // creates inventory  
        PrepareInventory(sellerInventoryData);
        sellerInventoryData.onInventoryUpdated += UpdateInventoryUI;
        PrepareInventory(playerInvenotry);
        playerInvenotry.onInventoryUpdated += UpdatePlayerInventoryUI;

        // prepares UI
        PrepareUI(inventoryUI, sellerInventoryData);

        PrepareUI(playerInventoryUI, playerInvenotry);
        PrepareUIButton();
        playerInventoryUI.CurUnit = player.Unit;
        playerUnit = player.Unit;
        ResetTransaction();
        if (transactionTextHolder != null) {
            transactionText = transactionTextHolder.GetComponentInChildren<TMP_Text>();
        }
    }
    public void Start() {
        ActivateUI(false);
    }

    public override void ActivateUI(bool isActive) {

        if (isActive) {
            inventoryUI.Show();
            playerInventoryUI.Show();
            ResetTransaction();
            buttons.SetActive(isActive);
            transactionTextHolder.SetActive(isActive);
            //updates UI to match data
            initialPlayerInventory = playerInvenotry.GetCurrentInventoryStateWithEmpty();
            initialSellerInventory = sellerInventoryData.GetCurrentInventoryStateWithEmpty();
            inventoryUI.ResetAllItems();
            foreach (var item in initialSellerInventory) {
                if (item.Value.IsEmpty()) {
                    continue;
                }
                inventoryUI.UpdateData(item.Key,
                item.Value.item.GetImage(),
                item.Value.count);
            }
            playerInventoryUI.ResetAllItems();
            foreach (var item in initialPlayerInventory) {
                if (item.Value.IsEmpty()) {
                    continue;
                }
                playerInventoryUI.UpdateData(item.Key,
                item.Value.item.GetImage(),
                item.Value.count);
            }

            isOpen = true;
        }
        else {
            inventoryUI.Hide();
            playerInventoryUI.Hide();
            isOpen = false;
            ResetTransaction();
            buttons.SetActive(isActive);
            transactionTextHolder.SetActive(isActive);
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

    public void ResetTransaction() {
        costOwed = 0;
        playerOwedItems = new List<InventoryItem>();
        sellerOwedItems = new List<InventoryItem>();
        initialSellerInventory = null;
        initialPlayerInventory = null;
        transactionText.text = ("Owed: " + costOwed);
        savedOwed = costOwed;
    }

    public void RevertTransaction() {
        // resets seller inventory UI
        inventoryUI.ResetAllItems();
        foreach (var item in initialSellerInventory) {
            sellerInventoryData.SetItemAt(item.Key, item.Value);
            if (item.Value.IsEmpty()) {
                continue;
            }
            inventoryUI.UpdateData(item.Key,
            item.Value.item.GetImage(),
            item.Value.count);
        }
        // resets player inventory UI
        playerInventoryUI.ResetAllItems();
        foreach (var item in initialPlayerInventory) {
            playerInvenotry.SetItemAt(item.Key, item.Value);
            if (item.Value.IsEmpty()) {
                continue;
            }
            playerInventoryUI.UpdateData(item.Key,
            item.Value.item.GetImage(),
            item.Value.count);
        }
        costOwed = savedOwed;
        transactionText.text = ("Owed: " + costOwed);

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
        Dictionary<int, InventoryItem> inventorySate = sellerInventoryData.GetCurrentInventoryState();
        foreach (var item in inventorySate) {
            playerInvenotry.AddItem(item.Value);
            sellerInventoryData.RemoveItem(item.Key, item.Value.count);
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
        curInventoryUI.onItemActionRequest += HandleItemActionRequest;
        curInventoryUI.onDescriptionClear += HandleDescriptionClear;
    }
    public void PrepareUIButton() {
        confirmButton.InitalizeButton("Confrim");
        confirmButton.onButtonClicked += HandleConfrimButtonPress;
        confirmButton.onButtonReleased += HandleButtonReleased;
        confirmButton.onPointerEnter += HandelSelectButton;
        confirmButton.onPointerExit += HandelDeselectButton;

        resetButton.InitalizeButton("Reset");
        resetButton.onButtonClicked += HandleResetButtonPress;
        resetButton.onButtonReleased += HandleButtonReleased;
        resetButton.onPointerEnter += HandelSelectButton;
        resetButton.onPointerExit += HandelDeselectButton;

        cancelButton.InitalizeButton("Cancel");
        cancelButton.onButtonClicked += HandleCancelButtonPress;
        cancelButton.onButtonReleased += HandleButtonReleased;
        cancelButton.onPointerEnter += HandelSelectButton;
        cancelButton.onPointerExit += HandelDeselectButton;


    }

    private void HandleConfrimButtonPress(ButtonUI button) {
        initialPlayerInventory = playerInvenotry.GetCurrentInventoryStateWithEmpty();
        initialSellerInventory = sellerInventoryData.GetCurrentInventoryStateWithEmpty();
        if (costOwed > 0) {
            playerInvenotry.RemoveItemBySO(money, costOwed);
            sellerInventoryData.AddItem(money, costOwed);
            playerUnit.Stats.Money -= costOwed;
            sellerUnit.Stats.Money += costOwed;
        }
        else {
            costOwed = costOwed * -1;
            sellerInventoryData.RemoveItemBySO(money, costOwed);
            playerInvenotry.AddItem(money, costOwed);
            playerUnit.Stats.Money += costOwed;
            sellerUnit.Stats.Money -= costOwed;
        }

        costOwed = 0;
        savedOwed = costOwed;
        transactionText.text = ("Owed: " + costOwed);
        button.Press();
    }
    private void HandleCancelButtonPress(ButtonUI button) {
        button.Press();
        RevertTransaction();
        ActivateUI(false);
    }
    private void HandleResetButtonPress(ButtonUI button) {
        RevertTransaction();
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
    }
    private void HandleTransferItems(InventorySO itemToTansterData, int transferIndex) {

        if (curInventory != null) {
            // Debug.Log("swapping " + curSelectedIndex + " from " + curInventory.name + " with " + transferIndex + " to " + itemToTansterData.name);
            // gets item that is being tranfered to another unit
            InventoryItem tempTransferItem = curInventory.GetItemAt(curSelectedIndex);
            if (curInventory == playerInvenotry) {
                costOwed -= tempTransferItem.count;
            }
            else {
                costOwed += tempTransferItem.count;
            }
            // gets item that is given back if any
            InventoryItem itemToTransfterBack = itemToTansterData.GetItemAt(transferIndex);
            curInventory.SetItemAt(curSelectedIndex, itemToTransfterBack);
            itemToTansterData.SetItemAt(transferIndex, tempTransferItem);


        }

    }

    private void HandleItemActionRequest(int curItemIndex, LootPageUI curPageUI) {

        InventorySO curInventory = curPageUI.DataOfInventory;
        InventoryItem tempItemSelected = curInventory.GetItemAt(curItemIndex);

        if (tempItemSelected.IsEmpty() || curPageUI == null) {
            return;
        }
        // if player is selling item
        if (curInventory == playerInvenotry && sellerUnit != null) {

            // if seller's inventory is full
            if (((tempItemSelected.item.Weight + sellerInventoryData.Weight) > sellerUnit.Stats.MaxWeight)) {
                Debug.Log((tempItemSelected.item.Weight + sellerInventoryData.Weight) + " is to heavy for " + sellerUnit.Stats.MaxWeight);
                return;
            }
            costOwed -= tempItemSelected.item.Value;

            // seller is sold item 
            sellerOwedItems.Add(tempItemSelected);
            // if player is selling and item they just bought
            if (playerOwedItems.Contains(tempItemSelected)) {
                playerOwedItems.Remove(tempItemSelected);
            }
            sellerInventoryData.AddItem(tempItemSelected.item, 1, tempItemSelected.itemState, tempItemSelected.moveListState, tempItemSelected.level);
            playerInvenotry.RemoveItem(curItemIndex, 1);
            Debug.Log("--- " + sellerInventoryData.Weight);
            Debug.Log("--- " + playerInvenotry.Weight);

        }
        // if player is buying item
        else if (playerUnit != null) {

            // if players's inventory is full or if player doenst have enough money
            if ((tempItemSelected.item.Weight + playerInvenotry.Weight) > playerUnit.Stats.MaxWeight ||
                tempItemSelected.item.Value + costOwed > playerUnit.Stats.Money) {
                Debug.Log((tempItemSelected.item.Weight + playerInvenotry.Weight) + " is to heavy for " + playerUnit.Stats.MaxWeight);
                Debug.Log((tempItemSelected.item.Value + costOwed) + " is to expensive for " + playerUnit.Stats.Money);
                return;
            }
            costOwed += tempItemSelected.item.Value;
            // player buys item
            playerOwedItems.Add(tempItemSelected);
            // if player buys back sold item
            if (sellerOwedItems.Contains(tempItemSelected)) {
                sellerOwedItems.Remove(tempItemSelected);
            }
            playerInvenotry.AddItem(tempItemSelected.item, 1, tempItemSelected.itemState, tempItemSelected.moveListState, tempItemSelected.level);
            sellerInventoryData.RemoveItem(curItemIndex, 1);
            Debug.Log("--- " + sellerInventoryData.Weight);
            Debug.Log("--- " + playerInvenotry.Weight);
        }
        transactionText.text = ("Owed: " + costOwed);
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
        sellerInventoryData.RemoveItem(curItemIndex, count);
        inventoryUI.ResetSelected();
        audioSource.PlayOneShot(dropCLip);
    }


    public override void PreformAction(int curItemIndex) {
        InventoryItem inventoryItem = sellerInventoryData.GetItemAt(curItemIndex);
        // avoids empty slots
        if (inventoryItem.IsEmpty()) {
            return;
        }
        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null) {
            itemAction.PerformAction(gameObject, inventoryItem.itemState);
            audioSource.PlayOneShot(itemAction.actionSFX);
            if (sellerInventoryData.GetItemAt(curItemIndex).IsEmpty()) {
                inventoryUI.ResetSelected();
            }
        }
        IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null) {
            // Debug.Log("remvoing action");
            sellerInventoryData.RemoveItem(curItemIndex, 1);
        }
    }
    public void AssignSellerUnit(Unit curSellerUnit) {
        sellerUnit = curSellerUnit;
        inventoryUI.CurUnit = curSellerUnit;
        AssignInventoryData(inventoryUI.CurUnit.Inventory);
    }
    public void AssignInventoryData(InventorySO newInventory) {
        if (newInventory == null) {
            Debug.Log("given null inventory");
            return;
        }
        if (sellerInventoryData != null) {
            sellerInventoryData.onInventoryUpdated -= UpdateInventoryUI;
        }
        sellerInventoryData = newInventory;
        sellerInventoryData.onInventoryUpdated += UpdateInventoryUI;
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
