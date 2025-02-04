using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private CombatGrid combatGrid;
    private Tilemap tilemap;
    [SerializeField] private TilemapVisual tilemapVisual;
    //public Transform player;
    [SerializeField] private float nodeRadius, nodeDiameter, detectorOffest;
   // [SerializeField] private Vector2 worldSize;
    [SerializeField] private LayerMask unwalkalbeMask;
 //   [SerializeField] private int gridSizeX, gridSizeY;
    [SerializeField] private GameObject world;
   // [SerializeField] private GameObject gridPrefab;
   // private Tilemap tilemap;
    private Tilemap.TilemapObject.TilemapSprite tilemapSprite;
    private Vector3 worldBottomLeft;
   // [SerializeField] private List<Node> path;
    private float worldSizeX, worldSizeY;
    // Start is called before the first frame update
    void Awake()
    {
        worldSizeX = world.GetComponent<RectTransform>().rect.width;
        worldSizeY = world.GetComponent<RectTransform>().rect.height;
       // combatGrid = new CombatGrid(nodeRadius, worldSizeX, worldSizeY,unwalkalbeMask);
       // tilemap = new Tilemap((int)worldSizeX, (int)worldSizeY, 0.5f);
      //  tilemap.SetTilemapVisual(tilemapVisual);
      //  combatGrid.TileMap = tilemap;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public CombatGrid CombatGrid { 
        get { return combatGrid; } 
        set { combatGrid = value; }
    }
}
