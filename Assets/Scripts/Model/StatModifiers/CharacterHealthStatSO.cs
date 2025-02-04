using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Modifiers/CharacterHealthStatSO")]
public class CharacterHealthStatSO : CharcterStatModifierSO
{

    public override void AffectCharacter(GameObject character, float val)
    {
        // Health health = character.ge
        // get chracters health and modifly it
        UnitStats characterStats = character.GetComponent<UnitStats>();
        if (characterStats != null)
        {
            characterStats.Heal(val);
        }
    }

    public override void AffectUnit(ModifierData data, Unit user, Node targetNode, List<Unit> targets, Action<int> onSuccessful, Action<int> onFailed, float val)
    {
        throw new System.NotImplementedException();
    }
}
