using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/UnuseableItemSO")]
    public class UnuseableItemSO : ItemSO, IDestroyableItem
    {
        [SerializeField] private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Use";
        [field: SerializeField] public AudioClip actionSFX { get; private set; }

        public bool PerformAccessoryAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState)
        {
            return true;
        }

        public bool PerformAction2(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }

        public bool PerformArmourAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }

        public void PerformCaptureAction(PlayerController player)
        {

        }

        public bool PerformHelmetAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }

        public bool PerformWeapnAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            return true;
        }
    }
}
