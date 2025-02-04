using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitProfileListUI : MonoBehaviour {
    [SerializeField] protected ProfileImageUI item;
    [SerializeField] protected MouseFollower mouseFollower;
    [SerializeField] protected RectTransform container;
    protected List<ProfileImageUI> itemList = new List<ProfileImageUI>();
    public virtual event Action onDescriptionClear;
    public virtual event Action<int> onDescriptionRequest;
    protected ProfileImageUI curSelectedItem = null;
    protected int currentItemIndex = -1;

    public virtual void InitalizeItems(List<ProfileImageUI> listToAddTo, ProfileImageUI curItem) {
        if (curItem != null) {
            itemList.Add(curItem);
            // handles if pointer us over item
            curItem.onPointerEnter += HandlePointerEnter;
            // handles if ponter moves from being over item 
            curItem.onPointerExit += HandlePointerExit;
        }
    }

    protected virtual void CreateItemUI() {
        ProfileImageUI curItem = Instantiate(item);
        curItem.transform.SetParent(container);
        curItem.gameObject.transform.localScale = Vector3.one;
        InitalizeItems(itemList, curItem);
        //curItem.ItemContainer = this;

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
    public virtual void CreateInventoryUI(List<Unit> units) {
        // initlaizes invenotry based on how many cildren the controlPanel gameboject has.
        // this is determined in the unity editor
        // this collects all inventory item slots in the current scene
        int childCount = 0;
        if (units != null) {
            childCount = units.Count;
        }
        for (int i = 0; i < childCount; i++) {
            ProfileImageUI curItem = Instantiate(item);
            curItem.transform.SetParent(container);
            curItem.gameObject.transform.localScale = Vector3.one;
            curItem.SetData(units[i].Stats.ProfileImage, "");

            InitalizeItems(itemList, curItem);
            // curItem.AssignTeam(units[i].Team);
        }
    }
    public void AssignTeams(List<Unit> units) {
        int childCount = 0;
        if (units != null) {
            childCount = units.Count;
        }
        for (int i = 0; i < childCount; i++) {
            ProfileImageUI curItem = itemList[i];
            curItem.AssignTeam(units[i].Team);
            curItem.Unit = units[i];
        }
    }
    public virtual void ClearInventoryUI() {
        for (int i = 0; i < container.childCount; i++) {
            Destroy(container.GetChild(i).gameObject);
            itemList.Clear();
        }
    }

    protected virtual void HandlePointerExit(ProfileImageUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        if (index == -1) {
            return;
        }
        //currentItem.Deselect();
        if (onDescriptionClear != null) {
            // DeselectAllItems2();
            onDescriptionClear.Invoke();
        }

    }

    protected virtual void HandlePointerEnter(ProfileImageUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        if (index == -1) {
            return;
        }
        //currentItem.Select();
        if (onDescriptionRequest != null) {
            onDescriptionRequest.Invoke(index);
        }
    }
    public void RemoveUnit(int indexToRemove, Unit unitToRemove) {
        if (indexToRemove < 0 || indexToRemove >= container.childCount) {
            return;
        }
        foreach (ProfileImageUI curUnitProfile in itemList.ToList()) {
            if (curUnitProfile.Unit == unitToRemove) {
                curUnitProfile.onPointerEnter -= HandlePointerEnter;
                curUnitProfile.onPointerExit -= HandlePointerExit;
                itemList.Remove(curUnitProfile);
                container.GetChild(indexToRemove).gameObject.SetActive(false);
            }
        }
    }
    /*---------------------------------------------------------------------
    |  Method  HandleItemSelection(ProfileImageUI currentItem)
    |
    |  Purpose: Sends the inforamtion about the slot 
    |           being selected to inventory controller
    |
    |   Parameters: ProfileImageUI currentItem = the current slot being selected
    |
    |  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void HandleItemSelection(ProfileImageUI currentItem) {
        int index = itemList.IndexOf(currentItem);
        curSelectedItem = currentItem;
        if (index == -1) {
            return;
        }
    }


    public virtual void CreateDraggedItem(Sprite sprite, int count) {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(sprite, count);
    }





    protected virtual void ResetDraggedItem() {
        mouseFollower.Toggle(false);
        currentItemIndex = -1;
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
    public virtual void UpdateData(int itemIndex, Sprite itemImage, String title) {
        // checks if inbounds
        if (itemList.Count > itemIndex) {
            // update data
            itemList[itemIndex].SetData(itemImage, title);
        }

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
        gameObject.SetActive(false);
        ResetDraggedItem();

    }

    public virtual void ResetSelected() {
        DeselectAllItems();
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
        foreach (ProfileImageUI item in itemList) {
            item.Deselect();
        }
    }
    /*---------------------------------------------------------------------
    *  Method DeselectAllItems()
    *
    *   Purpose: Deselects all items and actiom menu 
    *   Parameters: none
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    protected virtual void DeselectAllItemsCombat() {
        // deselects all items 
        foreach (ProfileImageUI item in itemList) {
            item.DeselectedForCombat();
        }
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
        foreach (ProfileImageUI item in itemList) {
            item.Deselect();
        }
    }
    public List<ProfileImageUI> GetItemList() {
        return itemList;
    }

    public virtual void UpdateDescription(int curItemIndex, Sprite image, string name, string description) {
        //updates desctiption to currently slected item
        mouseFollower.SetDescriptionData(image, name, description);
        mouseFollower.ToggleDescription(true);
        //clears all current selections
        DeselectAllItems();
        itemList[curItemIndex].Select();
        // selects the desired slot
    }

    public void SelectCurUnit(int curIndex) {
        if (curIndex < 0 || curIndex >= itemList.Count) {
            return;
        }
        DeselectAllItems();
        itemList[curIndex].Select();
    }
    public void SelectCurUnitForComabt(Unit curUnit) {
        if (curUnit == null) {
            return;
        }
        DeselectAllItemsCombat();
        foreach (ProfileImageUI item in itemList) {
            if (item.Unit == curUnit) {
                item.SelectedForCombat();
            }
        }
    }

    public virtual void ResetAllItems() {
        foreach (var item in itemList) {
            item.ResetData();
            item.Deselect();
        }
    }
    public virtual void ResetAllItemsForComabt() {
        foreach (var item in itemList) {
            item.ResetData();
            item.DeselectedForCombat();
        }
    }
    public virtual void ResetDescription() {
        mouseFollower.ToggleDescription(false);
        mouseFollower.Description.ResetDescription();

    }
}
