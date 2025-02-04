using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class CharcterStatModifierSO : ScriptableObject
{
    [SerializeField] public Type type;

    [SerializeField]
    public enum Type
    {
        Speed,
        Strength,
        Magic,
        Vitality,
        Intelligence,
        Charisma,
        MaxMp,
        CurMp,
        CurHp,
        CurExp,
        MaxExp,
        Level,
        Money,
        MaxWeight,
        CurWeight,
        BurnChance,
        FreezeChance,
        PosionChance,
        ShockChance,
        BurnResistance,
        FreezeResistance,
        PosionResistance,
        ShockResistance,
        CaptureUnitRate,
        Heal,
        Damage,
        Burn,
        Freeze,
        Shock,
        Posion
    }
    public abstract void AffectCharacter(GameObject character, float val);
    public abstract void AffectUnit(ModifierData data, Unit user, Node targetNode, List<Unit> targets, Action<int> onSuccessful, Action<int> onFailed, float val);

}
