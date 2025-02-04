using Inventory.Model;
using System.Collections.Generic;
[System.Serializable]
public class UnitInvenotryData {
    public List<InventoryItem> unitInventory;
    public int size;
    public float weight;
    public UnitInvenotryData() {
        size = 40;
        weight = 0;
        unitInventory = new List<InventoryItem>(40);
    }
    public UnitInvenotryData(List<InventoryItem> unitInventory, int size, float weight) {
        this.unitInventory = unitInventory;
        this.size = size;
        this.weight = weight;
    }
    public UnitInvenotryData(int size) {
        this.unitInventory = new List<InventoryItem>(size);
        this.size = size;
        this.weight = 0;
    }
}
