using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : MonoBehaviour {
    [SerializeField] private WeaponItemSO weapon;
    [SerializeField] private EquipableItemSO helmet;
    [SerializeField] private EquipableItemSO armour;
    [SerializeField] private EquipableItemSO accessory;
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private InventorySO abilityList, equipmentList;
    [SerializeField] private List<ItemParameter> parametersToModify, itemCurrentState;
    [SerializeField] private List<Move> movelistState;

    /*---------------------------------------------------------------------
     *  Method SetWeapon(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) 
     *
     *  Purpose: grabs equipable item if equip is attempted
     *           if the item is really euipable it is moved to its corrent slot
     *           and its current state is recorded
     *
     *   Parameters: EquipableItemSO weaponItemSO= ref to equiple itme that is tyting to be equipped
     *                List<ItemParameter> itemState = parameter list
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetArmour(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) {

        if (!equipmentList.GetItemAt(1).IsEmpty()) {
            inventoryData.AddItem(equipmentList.GetItemAt(1));
        }
        equipmentList.AddArmour(weaponItemSO, 1, itemState);



        this.armour = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        // this.movelistState = new List<Move>();
        ModifyParameters();
        // ModifyAbility();
    }
    /*---------------------------------------------------------------------
     *  Method SetWeapon(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) 
     *
     *  Purpose: grabs equipable item if equip is attempted
     *           if the item is really euipable it is moved to its corrent slot
     *           and its current state is recorded
     *
     *   Parameters: EquipableItemSO weaponItemSO= ref to equiple itme that is tyting to be equipped
     *                List<ItemParameter> itemState = parameter list
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetAccessory(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) {
        if (!equipmentList.GetItemAt(2).IsEmpty()) {
            inventoryData.AddItem(equipmentList.GetItemAt(2));
        }
        equipmentList.AddAccessory(weaponItemSO, 1, itemState);
        this.accessory = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        // this.movelistState = new List<Move>();
        ModifyParameters();
        // ModifyAbility();
    }
    /*---------------------------------------------------------------------
     *  Method SetWeapon(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) 
     *
     *  Purpose: grabs equipable item if equip is attempted
     *           if the item is really euipable it is moved to its corrent slot
     *           and its current state is recorded
     *
     *   Parameters: EquipableItemSO weaponItemSO= ref to equiple itme that is tyting to be equipped
     *                List<ItemParameter> itemState = parameter list
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetHelmet(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) {
        if (!equipmentList.GetItemAt(0).IsEmpty()) {
            inventoryData.AddItem(equipmentList.GetItemAt(0));
        }
        equipmentList.AddHelmet(weaponItemSO, 1, itemState);
        this.helmet = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        // this.movelistState = new List<Move>();
        ModifyParameters();
        // ModifyAbility();
    }
    /*---------------------------------------------------------------------
    *  Method SetWeapon(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) 
    *
    *  Purpose: grabs equipable item if equip is attempted
    *           if the item is really euipable it is moved to its corrent slot
    *           and its current state is recorded
    *
    *   Parameters: EquipableItemSO weaponItemSO= ref to equiple itme that is tyting to be equipped
    *                List<ItemParameter> itemState = parameter list
    *
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void SetWeapon(WeaponItemSO weaponItemSO, List<ItemParameter> itemState) {
        if (!equipmentList.GetItemAt(3).IsEmpty()) {
            inventoryData.AddItem(equipmentList.GetItemAt(3));
        }
        equipmentList.AddWeapon(weaponItemSO, 1, itemState);
        this.weapon = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        // this.movelistState = new List<Move>();
        ModifyParameters();
        // ModifyAbility();
    }
    public void AddAbility(AbilityItemSO weaponItemSO, int level, List<Move> moveState) {

        if (abilityList != null && weaponItemSO != null) {
            //ModifyAbility(1);
            Debug.Log(" adding ability");
            abilityList.AddItem(weaponItemSO, 1, itemCurrentState, moveState, level);

        }

    }
    /*---------------------------------------------------------------------
     *  Method SetWeapon(EquipableItemSO weaponItemSO, List<ItemParameter> itemState) 
     *
     *  Purpose: grabs equipable item if equip is attempted
     *           if the item is really euipable it is moved to its corrent slot
     *           and its current state is recorded
     *
     *   Parameters: EquipableItemSO weaponItemSO= ref to equiple itme that is tyting to be equipped
     *                List<ItemParameter> itemState = parameter list
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void ModifyParameters() {
        foreach (var parameter in parametersToModify) {
            if (itemCurrentState.Contains(parameter)) {
                int index = itemCurrentState.IndexOf(parameter);
                float newValue = itemCurrentState[index].val + parameter.val;
                itemCurrentState[index] = new ItemParameter(parameter.itemParameter, newValue);
                // itemCurrentState.Add(new ItemParameter(parameter.itemParameter, newValue));
            }
        }
        //itemCurrentState.Add(parametersToModify[0]);
    }
    private void ModifyAbility(int level) {

        movelistState.Add(weapon.MoveList[level - 1]);
        // movelistState.Add(weapon.MoveList[0]);
    }

    public EquipableItemSO Helmet {
        get {
            return helmet;
        }
        set {
            helmet = value;
        }
    }
    public WeaponItemSO Weapon {
        get { return weapon; }
        set { weapon = value; }
    }

    public EquipableItemSO Armour {
        get { return armour; }
        set { armour = value; }
    }

    public EquipableItemSO Accessory {
        get { return accessory; }
        set { accessory = value; }
    }

    public InventorySO AbilityList {
        get { return abilityList; }
        set { abilityList = value; }
    }



}
