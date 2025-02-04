using System.Collections.Generic;
using UnityEngine;
namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/EquipableItemSO")]
    public class EquipableItemSO : ItemSO, IDestroyableItem, EquipmentItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField] public AudioClip actionSFX { get; private set; }
        [field: SerializeField] private float defense;
        public bool PerformAccessoryAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

            if (weaponSystem != null)
            {
                weaponSystem.SetAccessory(this, itemState == null ?
                    GetList() : itemState);

                return true;
            }
            return false;
        }

        public bool PerformArmourAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

            if (weaponSystem != null)
            {
                weaponSystem.SetArmour(this, itemState == null ?
                    GetList() : itemState);

                return true;
            }
            return false;
        }

        public bool PerformHelmetAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

            if (weaponSystem != null)
            {
                weaponSystem.SetHelmet(this, itemState == null ?
                    GetList() : itemState);

                return true;
            }
            return false;
        }


        public float Defense
        {
            get { return defense; }
        }

    }
}
