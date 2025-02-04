using System.Collections.Generic;
using UnityEngine;
namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/AbilityItemSO")]
    public class AbilityItemSO : ItemSO, IDestroyableItem, AbilityItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField] public AudioClip actionSFX { get; private set; }
        [field: SerializeField] private List<Move> moveList { get; set; }

        public bool PerformAbilityAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null)
            {
                weaponSystem.AddAbility(this, item.level, MoveState == null ?
                    MoveList : MoveState);

                return true;
            }
            return false;
        }

    }
}
