using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inventory.Model {
    public class BasicItemSO : ItemSO, IDestroyableItem, IItemAction {
        
        [SerializeField] private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Use";
        [field: SerializeField] public AudioClip actionSFX { get; private set; }

      
        public bool PerformAction(GameObject character, List<ItemParameter> itemState) {
            foreach (ModifierData data in modifiersData) {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }

      

        

    }
}
