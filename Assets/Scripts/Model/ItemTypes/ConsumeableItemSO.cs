using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/ConsumeableItemSO")]
    public class ConsumeableItemSO : ItemSO, IDestroyableItem, IItemAction
    {

        [SerializeField] private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Consume";
        [field: SerializeField] public AudioClip actionSFX { get; private set; }

        public bool PerformAccessoryAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }

        /*---------------------------------------------------------------------
        *  Method PerformAction(GameObject character)
        *
        *  Purpose: Perfrosm the actios of a character
        *
        * Parameters: GameObject character = chatcer whos actrins are preformed
        *
        * Returns: none
        *-------------------------------------------------------------------*/
        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            // Debug.Log("using");
            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }


    }
    /*---------------------------------------------------------------------
     *  Interface IDestroyableItem
     *
     *  Purpose: Allows items to be destroyed when selected
     *
     *-------------------------------------------------------------------*/
    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        abstract bool PerformAction(GameObject character, List<ItemParameter> itemState);

    }
    public interface EquipmentItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformArmourAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item);
        bool PerformHelmetAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item);
        abstract bool PerformAccessoryAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item);
    }
    public interface AbilityItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAbilityAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item);

    }
    public interface WeaponItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformWeapnAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item);
    }
    public interface CaptureItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        abstract void PerformCaptureAction(PlayerController player);

    }

    [Serializable]
    public class ModifierData
    {
        public CharcterStatModifierSO statModifier;
        public float value;
        public bool isDone = true;

    }
}