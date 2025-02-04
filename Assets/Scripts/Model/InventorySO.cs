
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model {
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject {

        [SerializeField] private List<InventoryItem> inventoryItems;
        [field: SerializeField] private int size { get; set; } = 40;

        [field: SerializeField] private float weight { get; set; } = 0;
        public event Action<Dictionary<int, InventoryItem>> onInventoryUpdated;
        public InventorySO() {
            inventoryItems = new List<InventoryItem>(40);
            size = 40;
            weight = 0;
        }
        public void Initialize() {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < size; i++) {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
            weight = 0;
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
        public int AddItem(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveList = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            // if adding non stackable item
            //Debug.Log("adding item");
            if (item.isStackable == false) {
                for (int i = 0; i < inventoryItems.Count; i++) {
                    //Debug.Log(" ______ adding item");
                    // looks for first free slot and adds items until
                    // count == 0 
                    // also if multiple unstabckable items are added,
                    // then "AddToFirstFreeSlot" will add items in free slots until
                    // none are left
                    while (count > 0 && IsInventoryFull() == false) {
                        //Debug.Log("adding  non stackable item");
                        count -= AddToFirstFreeSlot(item, 1, itemState, moveList, level);
                        //count--;
                    }
                    // updates UI to match data
                    InformAboutChange();
                    return count;
                }
            }
            // Debug.Log("adding  stackable item");
            // adding item that is stackable
            count = AddStackableItem(item, count);
            // updates UI to match data
            InformAboutChange();
            return count;
        }



        public int AddWeapon(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveList = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            //  Debug.Log("weapon equipped!!!");
            if (inventoryItems.Count < 4) {
                InformAboutChange();
                return 0;
            }
            else {
                InventoryItem newItem = new InventoryItem(item, count, itemState, moveList, level);
                inventoryItems[3] = newItem;
                //  Debug.Log("weapon equipped!!!");
                InformAboutChange();
                return 0;
            }
        }
        public int AddArmour(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveList = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            // Debug.Log("weapon equipped!!!");
            if (inventoryItems.Count < 4) {
                InformAboutChange();
                return 0;
            }
            else {
                InventoryItem newItem = new InventoryItem(item, count, itemState, moveList, level);
                inventoryItems[1] = newItem;
                // Debug.Log("weapon equipped!!!");
                InformAboutChange();
                return 0;
            }

        }
        public int AddAccessory(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveList = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            //Debug.Log("weapon equipped!!!");
            if (inventoryItems.Count < 4) {
                InformAboutChange();
                return 0;
            }
            else {
                InventoryItem newItem = new InventoryItem(item, count, itemState, moveList, level);
                inventoryItems[2] = newItem;
                //Debug.Log("weapon equipped!!!");
                InformAboutChange();
                return 0;
            }
        }
        public int AddHelmet(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveList = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            //Debug.Log("weapon equipped!!!");
            if (inventoryItems.Count < 4) {
                InformAboutChange();
                return 0;
            }
            else {
                InventoryItem newItem = new InventoryItem(item, count, itemState, moveList, level);
                inventoryItems[0] = newItem;
                // Debug.Log("weapon equipped!!!");
                InformAboutChange();
                return 0;
            }
            return 0;

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
        private int AddToFirstFreeSlot(ItemSO item, int count, List<ItemParameter> itemState = null, List<Move> moveState = null, int level = 1) {
            if (item == null || count < 1) {
                return 0;
            }
            List<ItemParameter> curItemState;
            List<Move> curMoveState;
            if (moveState == null) {
                curMoveState = new List<Move>();
                if (item != null) {

                    curMoveState = item.MoveList;
                }

            }
            else {
                curMoveState = moveState;
            }

            if (itemState == null) {
                curItemState = item.GetList();
            }
            else {
                curItemState = itemState;
            }
            InventoryItem newItem = new InventoryItem(item, count, curItemState, curMoveState, level);
            // looks for first free empty slot
            for (int i = 0; i < inventoryItems.Count; i++) {
                // adds new item with count of 'count' in free slot
                if (inventoryItems[i].IsEmpty()) {
                    //Debug.Log("item added");
                    inventoryItems[i] = newItem;

                    weight += newItem.item.Weight;

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
            foreach (InventoryItem item in inventoryItems) {
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
        private int AddStackableItem(ItemSO item, int count) {
            if (item == null || count < 1) {
                return 0;
            }
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

                        weight += inventoryItems[i].item.Weight;

                        count -= amountLeftToAdd;
                    }
                    // if curItem's count + the new item is < maxStackSize
                    else {
                        // adds item and updates the count 
                        inventoryItems[i] = inventoryItems[i].ChangeCount(inventoryItems[i].count + count);

                        weight += inventoryItems[i].item.Weight;

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
        }

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
        public Dictionary<int, InventoryItem> GetCurrentInventoryState() {
            Dictionary<int, InventoryItem> retrunVal = new Dictionary<int, InventoryItem>();
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
        public Dictionary<int, InventoryItem> GetCurrentInventoryStateWithEmpty() {
            Dictionary<int, InventoryItem> retrunVal = new Dictionary<int, InventoryItem>();
            for (int i = 0; i < inventoryItems.Count; i++) {

                retrunVal[i] = inventoryItems[i];

            }
            return retrunVal;
        }
        public int Count() {
            int count = 0;
            foreach (InventoryItem item in inventoryItems) {
                if (item.IsEmpty()) {
                    continue;
                }
                count += item.count;
            }
            return count;
        }
        public void SetItemAt(int itemIndex, InventoryItem item) {
            if (itemIndex < 0 || itemIndex >= inventoryItems.Count) {
                return;
            }
            weight -= inventoryItems[itemIndex].Weight;
            weight += item.Weight;
            inventoryItems[itemIndex] = item;
            InformAboutChange();
        }
        public InventoryItem GetItemAt(int curItemIndex) {
            if (curItemIndex >= inventoryItems.Count) {
                return new InventoryItem();
            }
            return inventoryItems[curItemIndex];
        }
        public void ClearIndex(int index) {
            inventoryItems[index] = new InventoryItem(null, 0);
            InformAboutChange();
        }
        public int AddItem(InventoryItem item) {
            if (item.IsEmpty()) {
                return 0;
            }
            int remainder = AddItem(item.item, item.count);
            return remainder;
        }

        public void SwapItems(int curItemIndex, int itemIndexSwap) {
            InventoryItem curItemTemp = inventoryItems[curItemIndex];
            InventoryItem curItemToSwapTemp = inventoryItems[itemIndexSwap];
            if (curItemTemp.item != null && curItemToSwapTemp.item == curItemTemp.item) {
                if (curItemTemp.item.isStackable && curItemToSwapTemp.item.isStackable) {
                    inventoryItems[itemIndexSwap] = inventoryItems[itemIndexSwap].ChangeCount(curItemToSwapTemp.count + curItemTemp.count);
                    RemoveItem(curItemIndex, curItemTemp.count);
                    return;
                }
            }
            inventoryItems[curItemIndex] = inventoryItems[itemIndexSwap];
            inventoryItems[itemIndexSwap] = curItemTemp;

            InformAboutChange();

        }

        private void InformAboutChange() {
            onInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }
        public void RemoveItemByItem(InventoryItem item, int count) {
            foreach (KeyValuePair<int, InventoryItem> curItem in GetCurrentInventoryState()) {
                if (curItem.Value.IsEqual(item)) {
                    RemoveItem(curItem.Key, count);
                    return;
                }
            }
        }
        public void RemoveItemBySO(ItemSO item, int count) {
            foreach (KeyValuePair<int, InventoryItem> curItem in GetCurrentInventoryState()) {
                if (curItem.Value.item == item) {
                    RemoveItem(curItem.Key, count);
                    return;
                }
            }
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

                    weight -= (inventoryItems[curItemIndex].item.Weight * amount);

                    inventoryItems[curItemIndex] = InventoryItem.GetEmptyItem();

                }
                // updates count of item is removed
                else {

                    weight -= (inventoryItems[curItemIndex].item.Weight * amount);

                    inventoryItems[curItemIndex] =
                        inventoryItems[curItemIndex].ChangeCount(remainder);


                }
                InformAboutChange();
            }
        }

        public void IncreaseItemCount(int itemIndex, int newCount) {
            if (inventoryItems[itemIndex].IsEmpty()) {
                return;
            }
            int countAdded = newCount - inventoryItems[itemIndex].count;
            if (countAdded <= 0) {
                weight += inventoryItems[itemIndex].item.Weight * countAdded;
            }
            else {
                weight -= inventoryItems[itemIndex].item.Weight * countAdded;
            }
            inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeCount(newCount);

            InformAboutChange();
        }

        public int GetSize() {
            return size;
        }
        public void SetSize(int size) {
            this.size = size;
        }
        public List<InventoryItem> InventoryItems { get { return inventoryItems; } set { inventoryItems = value; } }

        public float Weight { get { return weight; } set { weight = value; } }
    }
    // a struct to keep track of inventory itme data
    // uses a struct to make data more secure
    // and can only be asscessed by invenotrySO
    [Serializable]
    public struct InventoryItem {
        public int count;
        public ItemSO item;
        //public bool isEmpty => item == null;
        public List<ItemParameter> itemState;
        public List<Move> moveListState;
        public int level;

        public InventoryItem(ItemSO item, int count) {
            this.item = item;
            this.count = count;
            this.itemState = new List<ItemParameter>();
            this.moveListState = new List<Move>();
            //  moveListState.Add(item.MoveList.First());

            level = 1;
        }
        public InventoryItem(ItemSO item, int count, List<ItemParameter> itemState) {
            this.item = item;
            this.count = count;
            this.itemState = itemState;
            this.moveListState = new List<Move>();
            //  moveListState.Add(item.MoveList.First());
            level = 1;

        }
        public InventoryItem(ItemSO item, int count, List<ItemParameter> itemState, List<Move> moveListState) {
            this.item = item;
            this.count = count;
            this.itemState = itemState;
            this.moveListState = moveListState;

            level = 1;

        }
        public InventoryItem(ItemSO item, int count, List<ItemParameter> itemState, List<Move> moveListState, int level) {
            this.item = item;
            this.count = count;
            this.itemState = itemState;
            this.moveListState = moveListState;
            this.level = level;

        }

        public InventoryItem ChangeCount(int newCount) {

            return new InventoryItem(this.item, newCount, this.itemState);

        }

        public static InventoryItem GetEmptyItem() {
            return new InventoryItem(null, 0, new List<ItemParameter>());
        }

        public InventoryItem ChangeLevel(int newLevel) {
            return new InventoryItem(this.item, this.count, this.itemState, this.moveListState, this.level);
        }



        public bool IsEmpty() {
            if (item == null) {
                return true;
            }
            else {
                return false;
            }
        }

        public bool IsEqual(InventoryItem other) {
            if (this.item == other.item && this.moveListState == other.moveListState
                && this.level == other.level && this.itemState == other.itemState) {
                return true;
            }
            else {
                return false;
            }
        }
        public float Weight {
            get {
                if (item != null) {
                    return item.Weight;
                }
                return 0;
            }
            set {
                if (item != null) {
                    item.Weight = value;
                }
                return;
            }
        }


    }
}