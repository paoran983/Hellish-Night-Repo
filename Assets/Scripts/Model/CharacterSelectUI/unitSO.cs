using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public abstract class UnitSO : ScriptableObject
{

    [field: SerializeField] public bool isStackable { get; set; }
    [field: SerializeField] private bool multiHit { get; set; } = false;
    [field: SerializeField] private bool hitObjects { get; set; } = false;
    private int ID => GetInstanceID();
    [field: SerializeField] private int minEffectiveRange { get; set; } = 1;
    [field: SerializeField] private int maxEffectiveRange { get; set; } = 1;
    [field: SerializeField] private int range { get; set; } = 1;
    [field: SerializeField] private int accuracy { get; set; } = 1;
    [field: SerializeField] private int spreadRange { get; set; } = 1;
    [field: SerializeField] private int mpCost { get; set; } = 1;
    [field: SerializeField] private int moveCost { get; set; } = 1;
    [field: SerializeField] private string itemName { get; set; }
    [field: SerializeField][field: TextArea] public string description { get; set; }
    [field: SerializeField] private Sprite image { get; set; }
    [field: SerializeField] private List<MoveParameter> parameterList { get; set; }

    [field: SerializeField] private List<ModifierData> modifierData = new List<ModifierData>();

    [field: SerializeField] private RangeType rangeType;
    [field: SerializeField] private GameObject projectile;


    [SerializeField]
    public enum RangeType
    {
        Default,
        Line,
        Spread,
        Connect,
        AOE
    }
    public int GetID()
    {
        return ID;
    }
    public int Range
    {
        get { return range; }
        set { range = value; }
    }
    public int SpreadRange
    {
        get { return spreadRange; }
        set { spreadRange = value; }
    }
    public int Accuracy
    {
        get { return accuracy; }
        set { accuracy = value; }
    }
    public int MpCost
    {
        get { return mpCost; }
        set { mpCost = value; }
    }
    public string GetName()
    {
        return itemName;
    }
    public Sprite GetImage()
    {
        return image;
    }
    public List<MoveParameter> GetList()
    {
        return parameterList;
    }
    public List<ModifierData> ModifierData
    {
        get { return modifierData; }
        set { modifierData = value; }
    }
    public int MaxEffectiveRange { get { return maxEffectiveRange; } set { maxEffectiveRange = value; } }

    public int MinEffectiveRange { get { return minEffectiveRange; } set { minEffectiveRange = value; } }

    public RangeType Type { get { return rangeType; } }
    public GameObject Propjectile { get { return projectile; } }
    public int MoveCost { get { return moveCost; } }
    public bool MultiHit { get { return multiHit; } }
    public bool HitObjects { get { return hitObjects; } }

}


