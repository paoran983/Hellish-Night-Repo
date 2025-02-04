using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Modifiers/TalkToModiferSO")]
public class TalkToModiferSO : CharcterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        throw new System.NotImplementedException();
    }

    public override void AffectUnit(ModifierData data, Unit user, Node targetNode, List<Unit> targets, Action<int> onSuccessful, Action<int> onFailed, float val)
    {
        if (targetNode.Unit == null) return;
        TalkToUnit(user, targetNode.Unit, onSuccessful, onFailed);
        if (data != null)
        {
            data.isDone = true;
        }
    }
    public void TalkToUnit(Unit user, Unit target, Action<int> onTalkSuccessful, Action<int> onTalkFailed)
    {

        if (user == null || target == null)
        {
            return;
        }
        // user.TalkToUnit(target, onTalkSuccessful, onTalkFailed);
    }
}
