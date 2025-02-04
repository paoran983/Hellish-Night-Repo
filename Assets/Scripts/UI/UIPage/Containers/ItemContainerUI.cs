using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainerUI : MonoBehaviour {
    [SerializeField] protected InventoryItemUI item;
    [SerializeField] protected InventorySO dataOfInventory;
    [SerializeField] protected MouseFollower mouseFollower;
    [SerializeField] protected RectTransform container;
    [SerializeField] protected ItemActionMenu itemMenu;
    protected List<InventoryItemUI> itemList = new List<InventoryItemUI>();
    public virtual event Action<int> onItemActionRequest, onStartDrag, onDescriptionRequest, onDescriptionClear, onEndDrag;
    public virtual event Action<int, int> onSwapItems;
    protected bool isDragging;
    public virtual event Action<InventoryItemUI, InventoryItemUI, int, int> onTransferItems;
    protected Unit curUnit;
    public InventoryItemUI curSelectedItem = null;
    protected int currentItemIndex = -1;

    public virtual void InitalizeItems(List<InventoryItemUI> listToAddTo, InventoryItemUI curItem) {
        if (curItem != null) {
            itemList.Add(curItem);
            Debug.Log(curItem.type);
            // handles what happens if itemUI is left clicked
            curItem.onItemClicked += HandleShowItemActions;
            // handles what happens if itemUI being dragged
            curItem.onItemBeginDrag += HandleBeginDrag;
            // handesl what happens if itemUI is being dropped on another itemUI
            curItem.onItemDroppedOn += HandleSwap;
            // handles what happens when item is dropped after being dragged.
            curItem.onItemEndDrag += HandleEndDrg;
            // handles what happens if  itemUI right clicked
            curItem.onRightMouseBtnClick += HandleShowItemActions;
            // handles if pointer us over item
            curItem.onPointerEnter += HandlePointerEnter;
            // handles if ponter moves from being over item 
            curItem.onPointerExit += HandlePointerExit;


        }
    }

    protected virtual void CreateItemUI() {
        InventoryItemUI curItem = Instantiate(item);
        curItem.transform.SetParent(container);
        curItem.gameObject.transform.localScale = Vector3.one;
        InitalizeItems(itemList, curItem);
        curItem.ItemContainer = this;

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
    public virtual void CreateInventoryUI(InventorySO curDataOfInventory) {
        // initlaizes invenotry based on how many cildren the controlPanel gameboject has.
        // this is determined in the unity editor
        // this collects all inventory item slots in the current scene
        dataOfInventory = curDataOfInventory;
        int childCount = 9;
        if (dataOfInventory != null) {
            childCount = dataOfInventory.GetSize();
        }
        for (int i = 0; i < childCount; i++) {
            InventoryItemUI curItem = Instantiate(item);
            curItem.transform.SetParent(container);
            curItem.gameObject.transform.localScale = Vector3.one;
            InitalizeItems(itemList, curItem);
        }
    }
    public virtual void ClearInventoryUI() {
        for (int i = 0; i < container.childCount; i++) {
            Destroy(container.GetChild(i).gameObject);
            itemList.Clear();
        }
    }
    protected virtual void HandlePointerExit(InventoryItemUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        if (index == -1) {
            return;
        }
        currentItem.Deselect();
        if (onDescriptionClear != null && !isDragging) {
            DeselectAllItems2();
            onDescriptionClear.Invoke(index);
        }

    }

    protected virtual void HandlePointerEnter(InventoryItemUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        if (index == -1) {
            return;
        }
        currentItem.Select();
        if (onDescriptionRequest != null && !isDragging) {
            onDescriptionRequest.Invoke(index);
        }
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
    protected virtual void HandleItemSelection(InventoryItemUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        curSelectedItem = currentItem;
        if (index == -1) {
            return;
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
    protected virtual void HandleBeginDrag(InventoryItemUI currentItem) {

        int index = itemList.IndexOf(currentItem);
        if (index == -1) {
            mouseFollower.Toggle(false);
            return;
        }
        // grabs index of clicked on slot
        currentItemIndex = index;
        // selects current slot
        HandleItemSelection(currentItem);
        if (onStartDrag != null) {

            onStartDrag.Invoke(index);
        }
    }

    public virtual void CreateDraggedItem(Sprite sprite, int count) {
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
    protected virtual void HandleSwap(InventoryItemUI currentItem) {

        int index = itemList.IndexOf(currentItem);
        //  if item is invalid
        if (index <= -1 || currentItem == null || index >= itemList.Count) {
            return;
        }
        // if item is within bounds
        if (currentItemIndex >= 0) {
            InventoryItemUI itemToSwap = itemList[currentItemIndex];
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
    protected virtual void HandleEndDrg(InventoryItemUI currentItem) {
        ResetDraggedItem();
        if (onEndDrag != null) {
            onEndDrag.Invoke(0);
        }
    }

    protected virtual void ResetDraggedItem() {
        mouseFollower.Toggle(false);
        currentItemIndex = -1;
        isDragging = false;
        curSelectedItem = null;
    }

    /*---------------------------------------------------------------------
    *   Method UpdateData(int itemIndex, Sprite itemImage, int itemCount)
    *
    *   Purpose: Upadtes data of a specific inventoryItemUI
    *
    *   Parameters: int itemIndex = index of item to update
    *               Sprite itemImage = the image to update to
    *               int itemCount = the count to update to
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public virtual void UpdateData(int itemIndex, Sprite itemImage, int itemCount) {
        // checks if inbounds
        if (itemList.Count > itemIndex) {
            // update data
            itemList[itemIndex].SetData(itemImage, itemCount);
        }

    }

    protected virtual void HandleShowItemActions(InventoryItemUI currentItem) {
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

    public virtual void ResetSelected() {
        DeselectAllItems();
    }

    public virtual void AddAction(string actoionName, Action action) {
        itemMenu.AddButon(actoionName, action);
    }
    /*---------------------------------------------------------------------
    *  Method ShowItemAction(int currentItemIndex)
    *
    *   Purpose: shows action menu and moves it to current item clicked.
    *           is called bu inventoryUIController
    *
    *   Parameters: int currentItemIndex = index of current item clicked
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public virtual void ShowItemAction(int currentItemIndex) {
        itemMenu.Toggle(true);
        itemMenu.transform.position = itemList[currentItemIndex].transform.position;
    }

    /*---------------------------------------------------------------------
    *  Method DeselectAllItems()
    *
    *   Purpose: Deselects all items and actiom menu 
    *   Parameters: none
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void DeselectAllItems() {
        // deselects all items 
        foreach (InventoryItemUI item in itemList) {
            item.Deselect();
        }
        // hides item action menu
        itemMenu.Toggle(false);
    }

    /*---------------------------------------------------------------------
    *  Method DeselectAllItems2()
    *
    *   Purpose: Deselects all items but leaves actiom menu if dragging
    *
    *   Parameters: none
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void DeselectAllItems2() {
        // deselects all items 
        foreach (InventoryItemUI item in itemList) {
            item.Deselect();
        }
    }
    public List<InventoryItemUI> GetItemList() {
        return itemList;
    }

    public InventorySO DataOfInventory {
        get {
            return dataOfInventory;
        }
        set {
            dataOfInventory = value;
        }
    }

    public Unit CurUnit {
        get {
            return curUnit;
        }
        set {
            curUnit = value;
        }
    }
    public ItemActionMenu ItemMenu { get { return itemMenu; } }

    public virtual void UpdateDescription(int curItemIndex, Sprite image, string name, string description) {
        //updates desctiption to currently slected item
        mouseFollower.SetDescriptionData(image, name, description);
        mouseFollower.ToggleDescription(true);
        //clears all current selections
        DeselectAllItems();
        itemList[curItemIndex].Select();
        // selects the desired slot
    }

    public virtual void ResetAllItems() {
        foreach (var item in itemList) {
            item.ResetData();
            item.Deselect();
        }
    }
    public virtual void ResetDescription() {
        mouseFollower.ToggleDescription(false);
        mouseFollower.Description.ResetDescription();

    }
}




