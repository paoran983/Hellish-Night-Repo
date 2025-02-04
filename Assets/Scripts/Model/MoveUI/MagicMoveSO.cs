using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MagicMoveSO : MoveSO {
    public string ActionName => "Equip";

    [field: SerializeField] public AudioClip actionSFX { get; private set; }
    public bool PerformAction(GameObject character, List<MoveParameter> itemState = null) {
        AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();

        return false;


    }


}