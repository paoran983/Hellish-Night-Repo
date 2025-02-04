using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : ItemContainerUIController
    {
        [SerializeField] private InventoryPageUI inventoryUI;
        [SerializeField] private int inventorySize;
        [SerializeField] private InventorySO inventoryData, equippedAbilites, equippedItems;
        [SerializeField] private Unit initialUnit;
        [SerializeField] private PlayerController player;


        public void Start()
        {

            curUnit = initialUnit;
            if (curUnit != null)
            {
                InitliazeUnit(curUnit);
            }
            inventoryUI.AssignUnit(curUnit);
            PrepareUI();
            PrepareInventory();
            ActivateUI(false);
        }
        public override void ActivateUI(bool isActive)
        {


            if (isActive)
            {
                inventoryUI.Show();
                inventoryUI.SetStatText();

                //updates UI to match data
                foreach (var item in inventoryData.GetCurrentInventoryState())
                {
                    inventoryUI.UpdateData(item.Key,
                        item.Value.item.GetImage(),
                        item.Value.count, item.Value.item);
                }
                foreach (var item in equippedAbilites.GetCurrentInventoryState())
                {
                    inventoryUI.UpdateAbilityData(item.Key,
                        item.Value.item.GetImage(),
                        item.Value.count, item.Value.item);
                }
                foreach (var item in equippedItems.GetCurrentInventoryState())
                {
                    inventoryUI.UpdateEquipmentData(item.Key,
                        item.Value.item.GetImage(),
                        item.Value.count, item.Value.item);
                }
                isOpen = true;

                return;
            }
            else
            {
                inventoryUI.Hide();
                isOpen = false;
            }

        }
        public override void PrepareInventory()
        {

            //inventoryData.Initialize();
            //equippedItems.Initialize();
            if (inventoryData == null) { return; }
            inventoryData.onInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty())
                {
                    continue;
                }
                inventoryData.AddItem(item);
            }
            equippedAbilites.onInventoryUpdated += UpdateAbilityUI;
            equippedItems.onInventoryUpdated += UpdateEquipmentUI;
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
        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventorySate)
        {

            // resets previous information
            inventoryUI.ResetAllItems();
            // updates UI with current inventory data
            foreach (var item in inventorySate)
            {
                inventoryUI.UpdateData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count, item.Value.item);
            }
        }
        /*---------------------------------------------------------------------
        |  Method UpdateAbilityUI(Dictionary<int, InventoryItem> inventorySate)
        |
        |  Purpose: Updates the ability UI when data is changed in the model
        |
        |   Parameters: Dictionary<int, InventoryItem> inventorySate dict that 
        |               represents the current data in the inventory model
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void UpdateAbilityUI(Dictionary<int, InventoryItem> inventorySate)
        {

            // resets previous information
            inventoryUI.ResetAllAbilites();
            // updates UI with current inventory data
            foreach (var item in inventorySate)
            {
                inventoryUI.UpdateAbilityData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count, item.Value.item);
            }
        }
        /*---------------------------------------------------------------------
        |  Method UpdateEquipmentUI(Dictionary<int, InventoryItem> inventorySate)
        |
        |  Purpose: Updates the equipment UI when data is changed in the model
        |
        |   Parameters: Dictionary<int, InventoryItem> inventorySate dict that 
        |               represents the current data in the inventory model
        |
        |  Returns: none
        *-------------------------------------------------------------------*/
        private void UpdateEquipmentUI(Dictionary<int, InventoryItem> inventorySate)
        {

            // resets previous information
            inventoryUI.ResetAllEquipment();
            // updates UI with current inventory data
            foreach (var item in inventorySate)
            {
                inventoryUI.UpdateEquipmentData(item.Key,
                    item.Value.item.GetImage(),
                    item.Value.count, item.Value.item);
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
        public override void PrepareUI()
        {

            inventoryUI.CreateInventoryUI(inventorySize);
            inventoryUI.onDescriptionRequest += HandleDescriptionRequest;
            inventoryUI.onSwapItems += HandleSwapItems;
            inventoryUI.onStartDrag += HandleDragging;
            inventoryUI.onItemActionRequest += HandleItemActionRequest;
            inventoryUI.onTransferItems += HandleTransferItemsl;
            inventoryUI.onDescriptionClear += HandleDescriptionClear;


        }



        private void HandleDescriptionClear(int obj, InventoryItemUI itemData)
        {
            inventoryUI.ResetDescription();
        }

        private void HandleTransferItemsl(InventoryItemUI item1UI_1, InventoryItemUI itemUI_2, int itemIndex1, int itemIndex2)
        {
            //Debug.Log(" -**- switching invenoty");
            InventorySO inventory1 = item1UI_1.GetParentInventory();
            InventorySO inventory2 = itemUI_2.GetParentInventory();
            InventorySO temp = inventory1;

        }

        protected override void HandleItemActionRequest(int curItemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(curItemIndex);
            // avoids empty slots
            if (inventoryItem.IsEmpty())
            {
                return;
            }
            // opens item menu
            inventoryUI.ShowItemAction(curItemIndex);
            // adds item specific action
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                inventoryUI.AddAction(itemAction.ActionName, () => PreformAction(curItemIndex));
            }
            EquipmentItemAction equipItemAction = inventoryItem.item as EquipmentItemAction;
            if (equipItemAction != null)
            {
                inventoryUI.AddAction(equipItemAction.ActionName, () => PreformAction(curItemIndex));
            }
            AbilityItemAction abilityItemAction = inventoryItem.item as AbilityItemAction;
            if (abilityItemAction != null)
            {
                inventoryUI.AddAction(abilityItemAction.ActionName, () => PreformAction(curItemIndex));
            }
            CaptureItemAction captureItemAction = inventoryItem.item as CaptureItemAction;
            if (captureItemAction != null)
            {
                inventoryUI.AddAction(captureItemAction.ActionName, () => PreformAction(curItemIndex));
            }
            WeaponItemAction weaponItemAction = inventoryItem.item as WeaponItemAction;
            // enures only one "equip" command is added
            if (weaponItemAction != null && equipItemAction == null)
            {
                inventoryUI.AddAction(weaponItemAction.ActionName, () => PreformAction(curItemIndex));
            }
            // adds drop option
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(curItemIndex, inventoryItem.count));
            }

        }

        private void DropItem(int curItemIndex, int count)
        {
            InventoryItem curItem = inventoryData.GetItemAt(curItemIndex);
            if (curItem.IsEmpty())
            {
                return;
            }
            if (curItem.item.type == ItemSO.Type.Money)
            {
                curUnit.Stats.Money -= count;
            }
            inventoryData.RemoveItem(curItemIndex, count);
            inventoryUI.ResetSelected();
            audioSource.PlayOneShot(dropCLip);
        }

        public override void PreformAction(int curItemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(curItemIndex);
            // avoids empty slots
            if (inventoryItem.IsEmpty())
            {
                return;
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);

                inventoryItem = new InventoryItem(inventoryItem.item, inventoryItem.count, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem.level + 1);
                // audioSource.PlayOneShot(itemAction.actionSFX);
                if (inventoryData.GetItemAt(curItemIndex).IsEmpty())
                {
                    inventoryUI.ResetSelected();
                }
            }

            CaptureItemAction captureItemAction = inventoryItem.item as CaptureItemAction;
            if (itemAction != null)
            {
                if (inventoryItem.item.type == ItemSO.Type.Capture)
                {
                    captureItemAction.PerformCaptureAction(player);
                }
                inventoryItem = new InventoryItem(inventoryItem.item, inventoryItem.count, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem.level + 1);
                // audioSource.PlayOneShot(itemAction.actionSFX);
                if (inventoryData.GetItemAt(curItemIndex).IsEmpty())
                {
                    inventoryUI.ResetSelected();
                }
            }
            EquipmentItemAction equipItemAction = inventoryItem.item as EquipmentItemAction;
            if (equipItemAction != null)
            {
                if (inventoryItem.item.type == ItemSO.Type.Armour)
                {
                    equipItemAction.PerformArmourAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
                }
                else if (inventoryItem.item.type == ItemSO.Type.Accessory)
                {
                    equipItemAction.PerformAccessoryAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
                }
                else if (inventoryItem.item.type == ItemSO.Type.Helmet)
                {
                    equipItemAction.PerformHelmetAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
                }
                else if (inventoryItem.item.type == ItemSO.Type.Helmet)
                {
                    equipItemAction.PerformHelmetAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
                }
            }
            AbilityItemAction abilityItemAction = inventoryItem.item as AbilityItemAction;
            if (abilityItemAction != null)
            {
                if (inventoryItem.item.type == ItemSO.Type.Ability)
                {
                    abilityItemAction.PerformAbilityAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
                }
            }
            WeaponItemAction weaponItemAction = inventoryItem.item as WeaponItemAction;
            if (weaponItemAction != null)
            {
                weaponItemAction.PerformWeapnAction(gameObject, inventoryItem.itemState, inventoryItem.moveListState, inventoryItem);
            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(curItemIndex, 1);
            }

            inventoryUI.ItemMenu.Toggle(false);
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
        protected override void HandleDragging(int curItemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(curItemIndex);
            // avoids empty slots
            if (inventoryItem.IsEmpty())
            {

                return;
            }
            // creates dragged item 
            inventoryUI.CreateDraggedItem(inventoryItem.item.GetImage(), inventoryItem.count);
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
        protected override void HandleSwapItems(int curitemIndex, int itemIndexSwap)
        {
            inventoryData.SwapItems(curitemIndex, itemIndexSwap);
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
        private void HandleDescriptionRequest(int curItemIndex, InventoryItemUI itemData)
        {
            InventoryItem curInventoryItem = new InventoryItem();
            // if getting description of eqiupped item
            if (itemData != null && curItemIndex >= inventoryData.GetSize())
            {
                if (itemData.type == InventoryItemUI.State.Equipped)
                {
                    curItemIndex -= inventoryData.GetSize();
                    curInventoryItem = equippedItems.GetItemAt(curItemIndex);
                    UpdateDescription(curInventoryItem, curItemIndex, itemData);
                    return;

                }
                else if (itemData.type == InventoryItemUI.State.Ability)
                {
                    curItemIndex -= inventoryData.GetSize() + equippedItems.GetSize();
                    curInventoryItem = equippedAbilites.GetItemAt(curItemIndex);
                    UpdateDescription(curInventoryItem, curItemIndex, itemData);
                    return;

                }
            }
            if (curItemIndex < 0 || curItemIndex >= inventoryData.GetSize())
            {
                Debug.Log("tout of bounds of " + curItemIndex);
                return;
            }

            curInventoryItem = inventoryData.GetItemAt(curItemIndex);
            UpdateDescription(curInventoryItem, curItemIndex, itemData);
            return;
        }

        private void UpdateDescription(InventoryItem curInventoryItem, int curItemIndex, InventoryItemUI curItemData)
        {
            // avoids empty slots
            if (curInventoryItem.IsEmpty())
            {
                inventoryUI.ResetDescription();
                return;
                // return;
            }
            // updates description page
            ItemSO curItem = curInventoryItem.item;
            inventoryUI.UpdateDescription(curItemIndex, curItem.GetImage(),
                curItem.GetName(), PrepareDescription(curInventoryItem), curItemData);

        }

        public void InitliazeUnit(Unit unit)
        {
            curUnit = unit;
            if (unit != null)
            {
                inventoryData = unit.Inventory;
                equippedAbilites = unit.AbilityList;
                equippedItems = unit.EquipmentList;

                inventoryUI.ProfileImage.sprite = curUnit.Stats.ProfileImage;
                curUnit.Stats.SetHealth();
                curUnit.Stats.SetMana();
                //inventoryUI.ResetAllItems();
            }
        }

        public void AssignUnit(Unit unit)
        {
            /*if (curUnit != null) {
                curUnit.Stats.HealthBars.Remove(inventoryUI.HealthBar);
            }*/
            curUnit = unit;
            if (unit != null)
            {
                inventoryData.onInventoryUpdated -= UpdateInventoryUI;
                inventoryData = unit.Inventory;
                inventoryData.onInventoryUpdated += UpdateInventoryUI;
                equippedAbilites.onInventoryUpdated -= UpdateAbilityUI;
                equippedAbilites = unit.AbilityList;
                equippedAbilites.onInventoryUpdated += UpdateAbilityUI;
                equippedItems.onInventoryUpdated -= UpdateEquipmentUI;
                equippedItems = unit.EquipmentList;
                equippedItems.onInventoryUpdated += UpdateEquipmentUI;
                inventoryUI.ProfileImage.sprite = curUnit.Stats.ProfileImage;
                inventoryUI.ResetAllItems();
                /* if(!curUnit.Stats.HealthBars.Contains(inventoryUI.HealthBar)) {
                     curUnit.Stats.HealthBars.Add(inventoryUI.HealthBar);
                 }
                 curUnit.Stats.SetHealth();*/
            }
        }

        public InventorySO InventoryData
        {
            get
            {
                return inventoryData;
            }
            set
            {
                inventoryData = value;
            }
        }
    }
}
