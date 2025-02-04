[System.Serializable]
public class UnitStatsData {
    public float maxHealth;
    public int maxMoveDistance;
    public int maxAttackDistance;
    public int speed;
    public int strength;
    public int magic;
    public int durability;
    public int vitality;
    public int intelligence;
    public int charisma;
    public float maxMP;
    public float curMP;
    public float curExp;
    public float maxExp;
    public int level;
    public int money;
    public float maxWeight;
    public float curWeight;
    public float burnChance;
    public float freezeChance;
    public float posionChance;
    public float shockChance;
    public bool isBurned;
    public bool isPosioned;
    public bool isShocked;
    public bool isFrozen;
    public float burnResistance;
    public float freezeResistance;
    public float posionResistance;
    public float shockResistance;
    public int captureUnitRate;
    public int charmRate;
    public int intimidateRate;
    public int trickRate;
    // public Sprite profileImage;
    public int tempSpeed, tempStrength, tempMagic, tempDurability, tempVitality, tempInteligence, tempCharisma;
    public float curHealth;

    public UnitStatsData(UnitStats stats) {
        this.maxHealth = stats.Maxhealth;
        this.maxAttackDistance = stats.MaxAttackDistance;
        this.maxMoveDistance = stats.MaxMoveDistance;
        this.maxExp = stats.MaxExp;
        this.level = stats.Level;
        this.money = stats.Money;
        this.curWeight = stats.CurWeight;
        this.burnChance = stats.BurnChance;
        this.freezeChance = stats.FreezeChance;
        this.posionChance = stats.PosionChance;
        this.shockChance = stats.ShockChance;
        this.burnResistance = stats.BurnResistance;
        this.speed = stats.Speed;
        this.strength = stats.Strength;
        this.magic = stats.Magic;
        this.durability = stats.Durability;
        this.vitality = stats.Vitality;
        this.intelligence = stats.Intelligence;
        this.charisma = stats.Charisma;
        this.maxMP = stats.MaxMP;
        this.curMP = stats.CurMP;
        this.curExp = stats.CurExp;
        this.maxExp = stats.MaxExp;
        this.maxWeight = stats.MaxWeight;
        this.isBurned = stats.IsBurned;
        this.isFrozen = stats.IsFrozen;
        this.freezeResistance = stats.FreezeResistance;
        this.shockResistance = stats.ShockResistance;
        this.posionResistance = stats.PosionResistance;
        this.captureUnitRate = stats.CaptureUnitRate;
        this.charmRate = stats.CharmRate;
        this.intimidateRate = stats.IntimidateRate;
        this.trickRate = stats.TrickRate;
        // this.profileImage = stats.ProfileImage;


    }
}
