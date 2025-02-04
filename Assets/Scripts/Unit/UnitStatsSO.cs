using Inventory.Model;
using UnityEngine;
[CreateAssetMenu]
public class UnitStatsSO : ScriptableObject
{
    [SerializeField] public float maxHealth;
    [SerializeField] public int maxMoveDistance;
    [SerializeField] public int maxAttackDistance;
    [SerializeField] public int speed;
    [SerializeField] public int strength;
    [SerializeField] public int magic;
    [SerializeField] public int durability;
    [SerializeField] public int vitality;
    [SerializeField] public int intelligence;
    [SerializeField] public int charisma;
    [SerializeField] public float maxMP;
    [SerializeField] public float curMP;
    [SerializeField] public float curExp;
    [SerializeField] public float maxExp;
    [SerializeField] public int level;
    [SerializeField] public int money;
    [SerializeField] public float maxWeight;
    [SerializeField] public float curWeight;
    [Header("Status Effects")]
    [SerializeField] public float burnChance;
    [SerializeField] public float freezeChance;
    [SerializeField] public float posionChance;
    [SerializeField] public float shockChance;
    [SerializeField] public bool isBurned;
    [SerializeField] public bool isPosioned;
    [SerializeField] public bool isShocked;
    [SerializeField] public bool isFrozen;
    [SerializeField] public float burnResistance;
    [SerializeField] public float freezeResistance;
    [SerializeField] public float posionResistance;
    [SerializeField] public float shockResistance;
    [SerializeField] public int captureUnitRate;
    [Header("Personality")]
    [SerializeField] public ModifierData comboModifier;
    [SerializeField] public MoveSO specialMove;
    [SerializeField] public int charmRate;
    [SerializeField] public int intimidateRate;
    [SerializeField] public int trickRate;
    [SerializeField] public Sprite profileImage;
    [SerializeField] public StausEffect curStatusEffect;

    public enum StausEffect
    {
        None,
        Burned,
        Frozen,
        Shocked,
        Posioned
    }
    // private int tempSpeed, tempStrength, tempMagic, tempDurability, tempVitality, tempInteligence, tempCharisma;
    // [SerializeField] public float curHealth;

    public float curHealth;

}
