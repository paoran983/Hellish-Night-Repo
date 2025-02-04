using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPageUI : MonoBehaviour
{
    [SerializeField] private MoveUI item;
    [SerializeField] private MoveList dataOfInventorys2;
    [SerializeField] private MouseFollower mouseFollower;
    [SerializeField] private RectTransform container;
    public List<MoveUI> itemList = new List<MoveUI>();
    public event Action<int> onItemActionRequest, onStartDrag;
    public event Action<int, CharacterSelectPageUI> onDescriptionRequest, onDescriptionClear, onItemSelect;
    public event Action<int, int> onSwapItems;
    public event Action<MoveUI, MoveUI, int, int> onTransferItems;
    private int currentItemIndex = -1;
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
    public void CreateInventoryUI()
    {
        // initlaizes invenotry based on how many cildren the controlPanel gameboject has.
        // this is determined in the unity editor
        // this collects all inventory item slots in the current scene
        int childCount = dataOfInventorys2.GetSize();
        for (int i = 0; i < childCount; i++)
        {

            MoveUI curItem = Instantiate(item);
            curItem.transform.SetParent(container);
            curItem.gameObject.transform.localScale = Vector3.one;
            if (curItem != null)
            {
                itemList.Add(curItem);
                // handles what happens if itemUI is left clicked
                curItem.onItemClicked += HandleItemSelection;
                curItem.onRightMouseBtnClick += HandleShowItemActions;
                curItem.onPointerEnter += HandlePointEnter;
                curItem.onPointerExit += HandlePointExit;

            }

        }



    }
    private void HandlePointExit(MoveUI currentMove)
    {
        int index = itemList.IndexOf(currentMove);

        if (index == -1)
        {
            return;
        }

        // onDescriptionRequest?.Invoke(index);
        if (onDescriptionClear != null)
        {
            onDescriptionClear.Invoke(index, this);
        }
    }

    private void HandlePointEnter(MoveUI currentMove)
    {
        int index = itemList.IndexOf(currentMove);

        if (index == -1)
        {
            return;
        }

        // onDescriptionRequest?.Invoke(index);
        if (onDescriptionRequest != null)
        {
            onDescriptionRequest.Invoke(index, this);
        }
        else
        {
            Debug.Log("action is null");
        }
    }

    private void HandleSwapWithDifInvenotry(MoveUI uI1, MoveUI uI2)
    {
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
    private void HandleItemSelection(MoveUI currentItem)
    {
        int index = itemList.IndexOf(currentItem);

        if (index == -1)
        {
            return;
        }
        // onDescriptionRequest?.Invoke(index);
        if (onItemSelect != null)
        {
            onItemSelect.Invoke(index, this);
        }
    }




    public void UpdateData(int itemIndex, Sprite itemImage, int itemCount)
    {
        // checks if inbounds

        if (itemList.Count > itemIndex)
        {
            //Debug.Log("++++" + itemIndex);
            // update data
            // Debug.Log("++++" + itemList.Count);
            itemList[itemIndex].SetData(itemImage, itemCount);
        }

    }
    private void HandleShowItemActions(MoveUI currentItem)
    {
        int index = itemList.IndexOf(currentItem);

        //check if slot tring to preform is in bounds
        if (index <= -1)
        {
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
    public void Show()
    {
        gameObject.SetActive(true);
        // ensure the descriptiuon page is cleared when player opens inventory
        //    itemDescription.ResetDescription();
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
    public void Hide()
    {
        // itemMenu.Toggle(false);
        ResetDescription();
        gameObject.SetActive(false);

        //  ResetDraggedItem();

    }

    public void ResetSelected()
    {
        //  itemDescription.ResetDescription();
        DeselectAllItems();
    }
    private void DeselectAllItems()
    {
        // deselects all items 
        foreach (MoveUI item in itemList)
        {
            item.Deselect();
        }
        // hides item action menu
        // itemMenu.Toggle(false);
    }
    public List<MoveUI> ItemList
    {
        get
        {
            return itemList;
        }
    }

    public void UpdateDescription(int curItemIndex, Sprite image, string name, string description)
    {
        //updates desctiption to currently slected item
        mouseFollower.SetDescriptionData(image, name, description);
        mouseFollower.ToggleDescription(true);
        // itemDescription.SetDescription(image, name, description);
        //clears all current selections
        DeselectAllItems();
        // selects the desired slot
        itemList[curItemIndex].Select();
    }

    public void ResetDescription()
    {
        //  itemDescription.ResetDescription();
        mouseFollower.ToggleDescription(false);
        mouseFollower.Description.ResetDescription();
        DeselectAllItems();

    }

    public void ResetAllItems()
    {
        foreach (var item in itemList)
        {
            item.ResetData();
            item.Deselect();
        }
    }
    public MoveList MoveData
    {
        get
        {
            return dataOfInventorys2;
        }
        set { dataOfInventorys2 = value; }
    }

    public MouseFollower MouseFollower
    {
        get
        {
            return mouseFollower;
        }
        set
        {
            mouseFollower = value;
        }
    }
}
