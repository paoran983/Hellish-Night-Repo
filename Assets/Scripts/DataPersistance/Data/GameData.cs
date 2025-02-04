using UnityEngine;
[System.Serializable]
public class GameData {
    public Vector3 unitPos;
    public SerializableDictionary<string, Vector3> unitsPos;
    public SerializableDictionary<string, UnitInvenotryData> unitsInventory;
    public SerializableDictionary<string, UnitInvenotryData> unitsAbilities;
    public SerializableDictionary<string, UnitInvenotryData> unitsEquipment;
    public SerializableDictionary<string, UnitStatsData> unitsStats;
    public int curPlayerLevel;
    public long lastUpdated;

    public GameData() {
        unitPos = Vector3.zero;
        unitsPos = new SerializableDictionary<string, Vector3>();
        unitsInventory = new SerializableDictionary<string, UnitInvenotryData>();
        unitsAbilities = new SerializableDictionary<string, UnitInvenotryData>();
        unitsEquipment = new SerializableDictionary<string, UnitInvenotryData>();
        unitsStats = new SerializableDictionary<string, UnitStatsData>();
    }
}
