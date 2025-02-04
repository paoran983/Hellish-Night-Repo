using UnityEngine;

public class Node : IHeapItem<Node>
{

    [SerializeField] private Vector3 worldPos;
    [SerializeField] bool walkable;
    private int height;
    private int layerInt;
    private string layerName;
    private int gCost;
    private int hCost;
    private int gridX;
    private int gridY;
    private Node parent;
    private int heapIndex;
    private bool inRange;
    private Unit unit;
    private Grid<Node> grid;
    public Tile.Type type;
    private SerializableDictionary<int, Tile.Type> levels;
    private SerializableDictionary<int, Tile> tileLevels;
    private SerializableDictionary<int, bool> transitionStart;
    private SerializableDictionary<int, bool> transitionEnd;
    private SerializableDictionary<int, bool> stairRail;
    private SerializableDictionary<int, bool> blocked;
    public Node()
    {
        this.worldPos = Vector3.zero;
        this.walkable = true;
        this.gridX = 0;
        this.gridY = 0;
        this.inRange = false;
        this.grid = null;
        this.height = 0;
        this.layerInt = 0;
        this.layerName = "";

    }
    public Node(bool walkable, Vector3 worldPos, int layerInt, Tile.Type type, int gridX, int gridY)
    {
        this.worldPos = worldPos;
        this.walkable = walkable;
        this.gridX = gridX;
        this.gridY = gridY;
        this.inRange = false;
        this.grid = null;
        this.layerInt = layerInt;
        this.type = type;
    }
    public Node(bool walkable, Vector3 worldPos, int layerInt, Tile.Type type, SerializableDictionary<int, Tile.Type> levels, int gridX, int gridY)
    {
        this.worldPos = worldPos;
        this.walkable = walkable;
        this.gridX = gridX;
        this.gridY = gridY;
        this.inRange = false;
        this.grid = null;
        this.layerInt = layerInt;
        this.type = type;
        this.levels = levels;
    }
    public Node(bool walkable, Vector3 worldPos, int layerInt, Tile.Type type,
        SerializableDictionary<int, Tile.Type> levels,
        SerializableDictionary<int, Tile> tileLevels,
        SerializableDictionary<int, bool> transitionStart,
        SerializableDictionary<int, bool> transitionEnd,
         SerializableDictionary<int, bool> stairRail,
         SerializableDictionary<int, bool> walkableByLevel, int gridX, int gridY)
    {
        this.worldPos = worldPos;
        this.walkable = walkable;
        this.gridX = gridX;
        this.gridY = gridY;
        this.inRange = false;
        this.grid = null;
        this.layerInt = layerInt;
        this.type = type;
        this.levels = levels;
        this.tileLevels = tileLevels;
        this.transitionStart = transitionStart;
        this.transitionEnd = transitionEnd;
        this.stairRail = stairRail;
        this.blocked = walkableByLevel;
    }
    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY)
    {
        this.worldPos = worldPos;
        this.walkable = walkable;
        this.gridX = gridX;
        this.gridY = gridY;
        this.inRange = false;
        this.grid = null;
    }
    public Node(Grid<Node> grid, int gridX, int gridY)
    {
        this.worldPos = Vector3.zero;
        this.walkable = true;
        this.gridX = gridX;
        this.gridY = gridY;
        this.inRange = false;
        this.grid = grid;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }
    public Unit Unit
    {
        get
        {
            return unit;
        }
        set
        {
            unit = value;
        }
    }
    public Node Parent
    {
        get
        {
            return parent;
        }
        set
        {
            parent = value;
        }
    }
    public bool Walkable
    {
        get
        {
            return walkable;
        }
        set
        {
            walkable = value;
        }
    }
    public Vector3 WorldPos
    {
        get
        {
            return worldPos;
        }
        set
        {
            worldPos = value;
        }
    }
    public bool InRange
    {
        get
        {
            return inRange;
        }
        set
        {
            inRange = value;
        }
    }

    public int GCost
    {
        get
        {
            return gCost;
        }
        set
        {
            gCost = value;
        }
    }
    public Vector2 Coords { get { return new Vector2(gridX, gridY); } }

    public int HCost
    {
        get
        {
            return hCost;
        }
        set
        {
            hCost = value;
        }
    }
    public int GridX
    {
        get
        {
            return gridX;
        }
        set
        {
            gridX = value;
        }
    }
    public int GridY
    {
        get
        {
            return gridY;
        }
        set
        {
            gridY = value;
        }
    }
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public int Height { get { return height; } set { height = value; } }
    public int LayerInt { get { return layerInt; } set { layerInt = value; } }
    public string LayerName { get { return layerName; } set { layerName = value; } }
    public Tile.Type GetLevelType(int level)
    {
        Tile.Type type = 0;
        if (this.levels.TryGetValue(level, out type))
        {
            return type;
        }
        return 0;
    }
    public Tile GetLevelTile(int level)
    {
        Tile tile = null;
        if (this.tileLevels.TryGetValue(level, out tile))
        {
            return tile;
        }
        return null;
    }
    public bool isTransStart(int level)
    {
        bool res = false;
        if (this.transitionStart.TryGetValue(level, out res))
        {
            return res;
        }
        return false;
    }
    public bool containsTransStart()
    {
        return this.transitionStart.ContainsValue(true);
    }
    public bool containsTransEnd()
    {
        return this.transitionEnd.ContainsValue(true);
    }
    public bool isTransEnd(int level)
    {
        bool res = false;
        if (this.transitionEnd.TryGetValue(level, out res))
        {
            return res;
        }
        return false;
    }
    public bool isRail(int level)
    {
        bool res = false;
        if (this.stairRail.TryGetValue(level, out res))
        {
            return res;
        }
        return false;
    }
    public bool isBlocked(int level)
    {
        bool res = false;
        if (this.blocked.TryGetValue(level, out res))
        {
            return res;
        }
        return false;
    }
    public string ToStirng2()
    {
        string res = "";
        res += "(" + this.gridX + "," + this.gridY + ")";
        return res;
    }
    /*public void SetParent(Node parent) { 
        this.parent = parent;
    }
    public Node GetParent() {
        return this.parent;
    }
    public bool GetWalkable() {  
        return walkable; 
    }
    public void SetWalkable(bool walkable) {
        this.walkable = walkable;
    }
    public Vector3 GetWorldPos() {
        return worldPos;
    }
    public void SetWorldPos(Vector3 worldPos) {
        this.worldPos = worldPos;
    }
    public int GetGCost() {
        return gCost;
    }
    public int GetHCost() {
        return hCost;
    }
    public void SetGCost(int gCost) {
        this.gCost = gCost;
    }
    public void SetHCost(int hCost) {
        this.hCost = hCost;
    }
    
    public int GetX() {
        return gridX;
    }
    public int GetY() {
        return gridY;
    }*/

    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
