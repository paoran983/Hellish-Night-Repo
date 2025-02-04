using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<InvenotrySlot> invenotrySlots;
    public GameObject invenItemPrefab;
    private int selectedSlotIndex = -1;
    [SerializeField] private List<RectTransform> inventoryList;
    private InvenotrySlot selectedSlot;
    // Start is called before the first frame update
    public void Awake() {
        InitializeInventorySlots();
    }
    public void ChangeSelectedSlot(InvenotrySlot newSelected) {
       /* if (selectedSlotIndex >= 0) {
            invenotrySlots[selectedSlotIndex].Deselect();
        }
        invenotrySlots[newSelectedIndex].Select();
        selectedSlotIndex = newSelectedIndex;*/
       if(selectedSlot != null) {
            selectedSlot.Deselect();
        }
        newSelected.Select();
        selectedSlot = newSelected;
    }

    public void InitializeInventorySlots() {
        foreach(RectTransform curInvetory in inventoryList) {
            int slotCount = curInvetory.childCount;
            for(int i = 0; i < slotCount; i++) {
                InvenotrySlot curSlot = curInvetory.GetChild(i).GetComponent<InvenotrySlot>();
                invenotrySlots.Add(curSlot);
                curSlot.onPointClick += HandleSelected;
                if(curSlot.GetComponentInChildren<InvenItem>() != null) {
                    curSlot.GetComponentInChildren<InvenItem>().onBeginDragItem += HandleSelected;
                    curSlot.GetComponentInChildren<InvenItem>().onEndDragItem += HandleSelected;

                }
            }
        }
    }

    private void HandleSelected(InvenotrySlot slot) {
        Debug.Log(slot.gameObject.name);
        ChangeSelectedSlot(slot);


    }

    public bool AddItem(ItemSO item) {
        // looks for stackable items of same kind
        for (int i = 0; i < invenotrySlots.Count; i++) {
            InvenotrySlot slot = invenotrySlots[i];
            InvenItem curItem = slot.GetComponentInChildren<InvenItem>();
            // if item of same kind and still has space
            if (curItem != null && curItem.item == item && curItem.item.isStackable
                && curItem.count<curItem.item.GetMaxStackSize()) {
                curItem.incrementCount(1);
                curItem.RefreshCount();
                return true;
            }
        }
        // looks for empty slot
        for (int i=0;i<invenotrySlots.Count;i++) {
            InvenotrySlot slot = invenotrySlots[i];
            InvenItem curItem = slot.GetComponentInChildren<InvenItem>();
            if(curItem != null) {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;

    }
    public void SpawnNewItem(ItemSO item,InvenotrySlot slot) {
        GameObject newItem = Instantiate(invenItemPrefab, slot.transform);
        InvenItem invenItem = newItem.GetComponentInChildren<InvenItem>();
        invenItem.InitialiseItem(item);

    }
}
