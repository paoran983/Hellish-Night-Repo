using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MoveListSO : ScriptableObject {

    [SerializeField] private List<Move> inventoryItems;
    [field: SerializeField] private int size { get; set; } = 40;

    public event Action<Dictionary<int, Move>, MoveListSO> onInventoryUpdated;

    public void Initialize() {
        inventoryItems = new List<Move>();
        for (int i = 0; i < size; i++) {
            inventoryItems.Add(Move.GetEmptyItem());
        }
    }

    /*---------------------------------------------------------------------
    *  Method AddItem(ItemSO item, int count)
    *
    *  Purpose: Adds items to inventory and returns the amount added
    *
    *   Parameters: ItemSO item = item(s) to add
    *               int count = amount of item(s) to add
    *
    *  Returns: int count = amount of items left over after adding
    *-------------------------------------------------------------------*/
    public int AddItem(MoveSO item, int count, List<MoveParameter> itemState = null) {
        // if adding non stackable item
        Debug.Log("trypign to add " + item.name + "  " + inventoryItems.Count + "  " + IsInventoryFull() + "  " + item.isStackable + " " + count);
        if (item.isStackable == false) {
            for (int i = 0; i < inventoryItems.Count; i++) {
                // looks for first free slot and adds items until
                // count == 0 
                // also if multiple unstabckable items are added,
                // then "AddToFirstFreeSlot" will add items in free slots until
                // none are left
                while (count > 0 && IsInventoryFull() == false) {
                    Debug.Log(" >> trypign to add " + item.name + "  " + inventoryItems.Count + "  " + IsInventoryFull() + "  " + item.isStackable);
                    count -= AddToFirstFreeSlot(item, 1);
                    //count--;
                }
                // updates UI to match data
                InformAboutChange();
                return count;
            }
        }
        Debug.Log(" << trypign to add " + item.name + "  " + inventoryItems.Count + "  " + IsInventoryFull() + "  " + item.isStackable);
        // adding item that is stackable
        // updates UI to match data
        InformAboutChange();
        return count;
    }

    /*---------------------------------------------------------------------
    *  Method AddToFirstFreeSlot(ItemSO item, int count)
    *
    *  Purpose: Looks through inventory for the frist free slot
    *           Then adds item to slot and returns 
    *           the amount of items added
    *
    *   Parameters: ItemSO item = item(s) to add
    *               int count = amount of item(s) to add
    *
    *  Returns: int count or 0 = amount of items added
    *-------------------------------------------------------------------*/
    private int AddToFirstFreeSlot(MoveSO item, int count, List<MoveParameter> itemState = null) {
        List<MoveParameter> curItemState;
        if (itemState == null) {
            curItemState = item.GetList();
        }
        else {
            curItemState = itemState;
        }
        Move newItem = new Move(item, count, itemState);
        // looks for first free empty slot
        Debug.Log(" -- trypign to add " + item.name + "  " + inventoryItems.Count);
        for (int i = 0; i < inventoryItems.Count; i++) {
            // adds new item with count of 'count' in free slot
            if (inventoryItems[i].IsEmpty()) {
                inventoryItems[i] = newItem;
                Debug.Log("added moveSO " + newItem.item.name);
                // retruns amount of added items
                return count;
            }
        }
        return 0;
    }

    /*---------------------------------------------------------------------
    *  Method IsInventoryFull()
    *
    *  Purpose: Looks through inventory to see if it's full
    *
    *   Parameters: 
    *
    *  Returns: true or false = if invenotry is full
    *-------------------------------------------------------------------*/
    private bool IsInventoryFull() {
        foreach (Move item in inventoryItems) {
            // empty slot is found
            if (item.IsEmpty()) {
                return false;
            }
        }
        // every entry is full
        return true;
    }

    /*---------------------------------------------------------------------
    *  Method AddStackableItem(ItemSO item, int count)
    *
    *  Purpose: Tries to add stackable item(s) to invenotry.
    *           Does this by frist looking thorugh ther inventory and adding to 
    *           any mactbes found until count == 0 or there is nothing left to add.
    *           Then if there are left overs it looks through the invneotry again 
    *           looking for empty slots. Then adds items to slots until 
    *           the inventory if full or count == 0
    *
    *   Parameters: ItemSO item = item(s) to add
    *               int count = number of item(s) to add
    *
    *  Returns: int count or 0 = amounf of left over item(s) to add
    *-------------------------------------------------------------------*/
    /* private int AddStackableItem(MoveSO item, int count) {
         // loops through inventory slots
         for (int i = 0; i < inventoryItems.Count; i++) {
             // skips empty slots
             // InventoryItem curItem = inventoryItems[i];
             if (inventoryItems[i].IsEmpty()) {
                 continue;
             }
             // if matching item is found to add to stack
             if (inventoryItems[i].item.GetID() == item.GetID()) {
                 int amountLeftToAdd = inventoryItems[i].item.GetMaxStackSize() - inventoryItems[i].count;
                 // if amount to add is > than maxStackSize
                 if (count > amountLeftToAdd) {
                     // fills the currently found item and moves on 
                     // to add leftovers
                     inventoryItems[i] = inventoryItems[i].ChangeCount(inventoryItems[i].item.GetMaxStackSize());
                     count -= amountLeftToAdd;
                 }
                 // if curItem's count + the new item is < maxStackSize
                 else {
                     // adds item and updates the count 
                     inventoryItems[i] = inventoryItems[i].ChangeCount(inventoryItems[i].count + count);
                     InformAboutChange();
                     return 0;
                 }
             }
         }
         // looks to add left over items or if match can't be found
         while (count > 0 && IsInventoryFull() == false) {
             // splits the items up if count>MaxStackSize
             // chekcs if count is within range of maxStakcSize
             // if > maxStackSize then max is returned
             // if < 0 then 0 is given
             int newCount = Mathf.Clamp(count, 0, item.GetMaxStackSize());
             count -= newCount;
             // adds left overs to first free slot
             AddToFirstFreeSlot(item, newCount);
         }
         // return count of any leftover items
         return count;
     }*/

    /*---------------------------------------------------------------------
    *  Method IsInventoryFull()
    *
    *  Purpose: creates dictionary to keep track of current items and 
    *           there index in the invenotry model
    *
    *   Parameters: 
    *
    *  Returns: retrunVal = dictioary to represent data
    *-------------------------------------------------------------------*/
    public Dictionary<int, Move> GetCurrentInventoryState() {
        Dictionary<int, Move> retrunVal = new Dictionary<int, Move>();
        for (int i = 0; i < inventoryItems.Count; i++) {
            if (inventoryItems[i].IsEmpty()) {
                continue;
            }
            else {
                retrunVal[i] = inventoryItems[i];
            }
        }
        return retrunVal;
    }

    public Move GetItemAt(int curItemIndex) {
        return inventoryItems[curItemIndex];
    }

    public int GetMoveIndex(Move moveToFind) {
        return inventoryItems.IndexOf(moveToFind);
    }
    public void ClearIndex(int index) {
        inventoryItems[index] = new Move(null, 0);
        InformAboutChange();
    }
    public int AddItem(Move item) {
        int remainder = AddItem(item.item, item.count);
        return remainder;
    }

    public void SwapItems(int curItemIndex, int itemIndexSwap) {
        Move curItemTemp = inventoryItems[curItemIndex];
        inventoryItems[curItemIndex] = inventoryItems[itemIndexSwap];
        inventoryItems[itemIndexSwap] = curItemTemp;
        InformAboutChange();

    }

    private void InformAboutChange() {

        onInventoryUpdated?.Invoke(GetCurrentInventoryState(), this);
    }

    public void RemoveItem(int curItemIndex, int amount) {
        if (inventoryItems.Count > curItemIndex) {
            // avoid empty slots
            if (inventoryItems[curItemIndex].IsEmpty()) {
                return;
            }
            int remainder = inventoryItems[curItemIndex].count - amount;
            // clears slot if empty has one left
            if (remainder <= 0) {
                inventoryItems[curItemIndex] = Move.GetEmptyItem();
            }
            // updates count of item is used
            else {
                inventoryItems[curItemIndex] =
                    inventoryItems[curItemIndex].ChangeCount(remainder);


            }
            InformAboutChange();
        }
    }

    public int GetSize() {
        return size;
    }
}
// a struct to keep track of inventory itme data
// uses a struct to make data more secure
// and can only be asscessed by invenotrySO
[Serializable]
public struct Move {
    public int count;
    public MoveSO item;
    //public bool isEmpty => item == null;
    public List<MoveParameter> moveState;

    public Move(MoveSO item, int count) {
        this.item = item;
        this.count = count;
        this.moveState = new List<MoveParameter>();
    }
    public Move(MoveSO item, int count, List<MoveParameter> moveState) {
        this.item = item;
        this.count = count;
        this.moveState = moveState;
    }
    public Move ChangeCount(int newCount) {

        return new Move(item, newCount);

    }

    public static Move GetEmptyItem() {
        return new Move(null, 0);
    }

    public bool IsEmpty() {
        if (item == null) {
            return true;
        }
        else {
            return false;
        }
    }
}
