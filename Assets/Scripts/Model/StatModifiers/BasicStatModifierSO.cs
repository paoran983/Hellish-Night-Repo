using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Modifiers/BasicStatModifierSO")]
public class BasicStatModifierSO : CharcterStatModifierSO
{


    // Start is called before the first frame update
    public override void AffectCharacter(GameObject character, float val)
    {
        // Health health = character.ge
        // get chracters health and modifly it
        int moveRoll;
        UnitStats characterStats = character.GetComponent<UnitStats>();
        if (characterStats == null)
        {
            return;
        }
        switch (type)
        {
            case Type.Speed:
                characterStats.Speed += (int)val;
                break;
            case Type.Strength:
                characterStats.Strength += (int)val;
                break;
            case Type.Magic:
                characterStats.Magic += (int)val;
                break;
            case Type.Vitality:
                characterStats.Vitality += (int)val;
                break;
            case Type.Intelligence:
                characterStats.Intelligence += (int)val;
                break;
            case Type.Charisma:
                characterStats.Charisma += (int)val;
                break;
            case Type.MaxMp:
                characterStats.MaxMP += val;
                break;
            case Type.CurHp:
                characterStats.CurMP += val;
                break;
            case Type.CurExp:
                characterStats.CurExp += val;
                break;
            case Type.MaxExp:
                characterStats.MaxExp += val;
                break;
            case Type.Level:
                characterStats.Level += (int)val;
                break;
            case Type.Money:
                characterStats.Money += (int)val;
                break;
            case Type.MaxWeight:
                characterStats.MaxWeight += val;
                break;
            case Type.CurWeight:
                characterStats.CurWeight += val;
                break;
            case Type.BurnChance:
                characterStats.BurnChance += val;
                break;
            case Type.FreezeChance:
                characterStats.FreezeChance += val;
                break;
            case Type.PosionChance:
                characterStats.PosionChance += val;
                break;
            case Type.ShockChance:
                characterStats.ShockChance += val;
                break;
            case Type.BurnResistance:
                characterStats.BurnResistance += val;
                break;
            case Type.FreezeResistance:
                characterStats.FreezeResistance += val;
                break;
            case Type.PosionResistance:
                characterStats.PosionResistance += val;
                break;
            case Type.ShockResistance:
                characterStats.ShockResistance += val;
                break;
            case Type.CaptureUnitRate:
                characterStats.CaptureUnitRate += (int)val;
                break;
            case Type.Damage:
                characterStats.Damage(val);
                break;
            case Type.Heal:
                characterStats.Heal(val);
                break;
            case Type.Burn:
                moveRoll = UnityEngine.Random.Range(1, 100);
                if (moveRoll <= val + characterStats.BurnChance)
                {
                    characterStats.Burn(true);
                }
                break;
            case Type.Freeze:
                moveRoll = UnityEngine.Random.Range(1, 100);
                if (moveRoll <= val + characterStats.FreezeChance)
                {
                    characterStats.Freeze(true);
                }
                break;
            case Type.Shock:
                moveRoll = UnityEngine.Random.Range(1, 100);
                if (moveRoll <= val + characterStats.FreezeChance)
                {
                    characterStats.Shock(true);
                }
                break;
            case Type.Posion:
                moveRoll = UnityEngine.Random.Range(1, 100);
                if (moveRoll <= val + characterStats.PosionChance)
                {
                    characterStats.Posion(true);
                }
                break;
        }



    }

    public override void AffectUnit(ModifierData data, Unit user, Node targetNode, List<Unit> targets, Action<int> onSuccessful, Action<int> onFailed, float val)
    {
        AffectCharacter(user.gameObject, val);
    }
}
