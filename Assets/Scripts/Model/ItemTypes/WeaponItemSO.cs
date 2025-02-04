using System.Collections.Generic;
using UnityEngine;
namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Items/WeaponItemSO")]
    public class WeaponItemSO : ItemSO, IDestroyableItem, WeaponItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField] public AudioClip actionSFX { get; private set; }
        [field: SerializeField] private List<Move> moveList { get; set; }
        [field: SerializeField] private float accuracy;
        [field: SerializeField] private float damage;
        [field: SerializeField] private int minEffectiveRange;
        [field: SerializeField] private int maxEffectiveRange;
        [field: SerializeField] private int range;
        [field: SerializeField] private int moveCost;
        [field: SerializeField] private List<ModifierData> modifierData = new List<ModifierData>();
        public bool PerformWeapnAction(GameObject character, List<ItemParameter> itemState, List<Move> MoveState, InventoryItem item)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ?
                    GetList() : itemState);

                return true;
            }
            return false;
        }
        public float Accuracy
        {
            get { return accuracy; }
        }
        public float Damage
        {
            get
            {
                return damage;
            }
        }
        public int MinEffectiveRange { get { return minEffectiveRange; } }
        public int MaxEffectiveRange { get { return maxEffectiveRange; } }
        public int Range { get { return range; } }
        public int MoveCost { get { return moveCost; } }
        public List<ModifierData> ModifierData { get { return modifierData; } set { modifierData = value; } }
    }
}
