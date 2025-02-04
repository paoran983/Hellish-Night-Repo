using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Modifiers/TeleportModiferSO")]
public class TeleportModiferSO : CharcterStatModifierSO
{
    public int level = 1;
    public override void AffectCharacter(GameObject character, float val)
    {
        throw new System.NotImplementedException();
    }

    public override void AffectUnit(ModifierData data, Unit user, Node targetNode, List<Unit> targets, Action<int> onSuccessful, Action<int> onFailed, float val)
    {
        Teleport(targetNode, user, targets, val);
        if (data != null)
        {
            data.isDone = true;
        }
    }
    public void Teleport(Node targetNode, Unit user, List<Unit> targets, float val)
    {
        Debug.Log("TELEPORTING " + val);
        switch (val)
        {
            case 1:
                TeleportLevel_1(user, targets);
                break;
            case 2:
                TeleportLevel_2(user, targets);
                break;
            case 3:
                TeleportLevel_3(user, targetNode);
                break;


        }
    }
    public void TeleportLevel_1(Unit user, List<Unit> targets)
    {

        if (targets == null || user == null) return;
        Debug.Log("affects " + targets.Count + " units");
        if (targets.Count <= 0) return;
        Unit target = targets[0];
        if (target == null) return;
        if (user.CurNode == null || target.CurNode == null)
        {
            return;
        }
        Debug.Log("teleporting 1 swithcing " + target.name + " with " + user.name);
        Node lastUserNode = user.CurNode;
        user.CurNode = target.CurNode;
        target.CurNode = lastUserNode;
        user.transform.position = user.CurNode.WorldPos;
        target.transform.position = target.CurNode.WorldPos;

    }
    public void TeleportLevel_2(Unit user, List<Unit> targets)
    {

        if (user == null || targets == null)
        {
            return;
        }
        if (targets.Count != 2) return;
        Unit target1 = targets[0];
        Unit target2 = targets[1];
        if (target2.CurNode == null || target1.CurNode == null)
        {
            return;
        }
        Node lastUserNode = target2.CurNode;
        // assign target2 target1's node and assignes each noe to correpsonding unit
        target2.CurNode = target1.CurNode;
        target2.CurNode.Unit = target2;
        // assign target1 target2's node and assignes each noe to correpsonding unit
        target1.CurNode = lastUserNode;
        target1.CurNode.Unit = target1;
        target2.transform.position = target2.CurNode.WorldPos;
        target1.transform.position = target1.CurNode.WorldPos;

    }
    public void TeleportLevel_3(Unit user, Node target)
    {

        if (user == null || target == null)
        {
            return;
        }
        Debug.Log("teleporting to node " + target.Walkable);
        if (target.Walkable == false) return;
        Debug.Log("teleporting to node");
        // assignes target node to user
        Node lastUserNode = user.CurNode;
        user.CurNode = target;
        target.Unit = user;
        // frees users previos node
        lastUserNode.Unit = null;
        lastUserNode.Walkable = true;
        user.transform.position = user.CurNode.WorldPos;


    }
}
