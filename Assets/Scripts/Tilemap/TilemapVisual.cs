/*---------------------------------------------------------------------
 *  Class: TilemapVisual
 *
 *  Purpose: Controls the visuls of a tileMap by creating a mesh array of the size of the 
 *           tileMap grid. It then listnes to the tileMap for when a node is changed
 *           if it is the class calls functions to deal with the and update the visuals 
 *-------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

public class TilemapVisual : MonoBehaviour
{

    [System.Serializable]
    public struct TilemapSpriteUV
    {
        public Tilemap.TilemapObject.TilemapSprite tilemapSprite;
        public Vector2Int uv00Pixels;
        public Vector2Int uv11Pixels;
    }

    public struct UVCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }

    [SerializeField] private TilemapSpriteUV[] tilemapSpriteUVArray;
    [SerializeField] private GameObject world;
    [SerializeField] private CombatGrid combatGrid;
    [SerializeField] private float offset, nodeSize;
    [SerializeField] private Sprite image;
    [SerializeField] private TilemapSpriteUV noneTileSprite;
    private Grid<Tilemap.TilemapObject> grid;
    private MeshArray mesh;
    private bool updateMesh;
    private Dictionary<Tilemap.TilemapObject.TilemapSprite, UVCoords> uvCoordsDictionary;
    private float worldSizeX, worldSizeY;
    private void Awake()
    {
        worldSizeX = world.GetComponent<RectTransform>().rect.width;
        worldSizeY = world.GetComponent<RectTransform>().rect.height;
        Texture texture = GetComponent<MeshRenderer>().material.mainTexture;
        float textureWidth = 1;
        float textureHeight = 1;
        if (texture != null)
        {
            textureWidth = texture.width;
            textureHeight = texture.height;

        }

        uvCoordsDictionary = new Dictionary<Tilemap.TilemapObject.TilemapSprite, UVCoords>();
        // loads each tilemap sprite made in editor
        foreach (TilemapSpriteUV tilemapSpriteUV in tilemapSpriteUVArray)
        {
            uvCoordsDictionary[tilemapSpriteUV.tilemapSprite] = new UVCoords
            {
                uv00 = new Vector2(tilemapSpriteUV.uv00Pixels.x / textureWidth, tilemapSpriteUV.uv00Pixels.y / textureHeight),
                uv11 = new Vector2(tilemapSpriteUV.uv11Pixels.x / textureWidth, tilemapSpriteUV.uv11Pixels.y / textureHeight),
            };

        }
    }

    public int GetTilemapSpriteUVIndex(Tilemap.TilemapObject.TilemapSprite type)
    {
        for (int i = 0; i < tilemapSpriteUVArray.Length; i++)
        {
            if (tilemapSpriteUVArray[i].tilemapSprite == type)
            {
                return i;
            }
        }
        return -1;
    }

    public UVCoords GetUvSpriteCoord(Tilemap.TilemapObject.TilemapSprite type)
    {
        foreach (KeyValuePair<Tilemap.TilemapObject.TilemapSprite, UVCoords> sprite in uvCoordsDictionary)
        {
            if (sprite.Key == type)
            {
                return sprite.Value;
            }
        }
        return new UVCoords { uv00 = new Vector2(0, 0), uv11 = new Vector2(0, 0) };
    }
    /*---------------------------------------------------------------------
     *  Method: SetGrid(Tilemap tilemap, Grid<Tilemap.TilemapObject> grid
     *
     *  Purpose: Assigns the tielmap grid that controls the tileMapVisuals
     *           It also initlaizes the tileMapVisual mesh. As well as tell 
     *           the grid to call the fucntion Grid_OnGridValueChanged when a value is changed
     *           and call Grid_OnGridVisualChanged when a visual is changed, and 
     *           Tilemap_OnLoaded when tilemap.OnLoaded is triggered
     *  Parameters: Tilemap tilemap = the tileMap of the visual
     *              Grid<Tilemap.TilemapObject> grid = the underlying grid of the visaul
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetGrid(Tilemap tilemap, Grid<Tilemap.TilemapObject> grid)
    {
        this.grid = grid;
        InitializeTileMapVisual();
        grid.OnGridObjectChanged += Grid_OnGridValueChanged;
        grid.OnGridVisualChanged += Grid_OnGridVisualChanged;
        tilemap.OnLoaded += Tilemap_OnLoaded;

    }

    private void Tilemap_OnLoaded(object sender, System.EventArgs e)
    {
        updateMesh = true;
    }

    /*---------------------------------------------------------------------
     *  Method: Grid_OnGridValueChanged(object sender, Grid<Tilemap.TilemapObject>.OnGridObjectChangedEventArgs e)
     *
     *  Purpose: The function called when OnGridValueChanged is triggered by the grid
     *           The function takes the object/ node being changed.
     *  Parameters: object sender = the sender of the event
     *              Grid<Tilemap.TilemapObject>.OnGridObjectChangedEventArgs e = The node that triggered the event
     *                   it contains the x,y coordiantes of the object
     *                   
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void Grid_OnGridValueChanged(object sender, Grid<Tilemap.TilemapObject>.OnGridObjectChangedEventArgs e)
    {
        //updateMesh = true;
        //  Debug.Log("click "+e.x+" "+e.y);
        //  mesh.ChangeMeshArrayNode(e.x, e.y, new Vector2(1, 0));

    }

    /*---------------------------------------------------------------------
     *  Method: Grid_OnGridVisualChanged(object sender, Grid<Tilemap.TilemapObject>.OnGridVisualChangedEventArgs e)
     *
     *  Purpose: The function called when OnGridVisualChanged is triggered by the grid
     *           The function takes the object/ node being changed.
     *           Then the fucntion takes the visual given when the event is triggerd and 
     *           changes the material of the object/ node at [e.x,e.y]
     *           
     *  Parameters: object sender = the sender of the event
     *              Grid<Tilemap.TilemapObject>.OnGridVisualChangedEventArgs e = The node that triggered the event
     *                   it contains the x,y coordiantes of the object and the visual is wants to be changed to
     *                   
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void Grid_OnGridVisualChanged(object sender, Grid<Tilemap.TilemapObject>.OnGridVisualChangedEventArgs e)
    {
        mesh.ChangeMeshArrayNode(e.x, e.y, e.uv00, e.uv11);
        mesh.UpdateLayer("UI", 0);
        //  Debug.Log("CUR LAYER IS " + mesh.meshRender.sortingOrder + " " + mesh.meshRender.sortingLayerName + "  " + mesh.meshRender.sortingLayerID);
    }

    private void ChangeNOdeVIsual(int x, int y, Vector2 uv00, Vector2 uv11)
    {
        mesh.ChangeMeshArrayNode(x, y, uv00, uv11);
    }

    /*---------------------------------------------------------------------
     *  Method: InitializeTileMapVisual()
     *
     *  Purpose: Initalizes the tilemap visul by making an meshArray of the size
     *           of the tileMap grid.
     *  Parameters: none
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void InitializeTileMapVisual()
    {

        mesh = new MeshArray(GetComponent<MeshFilter>(), GetComponent<MeshRenderer>());
        worldSizeX = world.GetComponent<RectTransform>().rect.width;
        worldSizeY = world.GetComponent<RectTransform>().rect.height;
        mesh.CreateMeshGrid((int)worldSizeX, (int)worldSizeY, nodeSize, offset);
        for (int x = 0; x < grid.GridSizeX; x++)
        {
            for (int y = 0; y < grid.GridSizeY; y++)
            {
                int index = x * grid.GridSizeY + y;
                Vector3 quadSize = new Vector3(1, 1) * grid.NodeDiameter;

                Tilemap.TilemapObject gridObject = grid.GetGridObject(x, y);
                Tilemap.TilemapObject.TilemapSprite tilemapSprite = gridObject.GetTilemapSprite();
                Vector2 gridUV00, gridUV11;
                gridObject.SetTilemapSprite(Tilemap.TilemapObject.TilemapSprite.None);
                // sets tielmap visual to none at first
                gridUV00 = noneTileSprite.uv00Pixels;
                gridUV11 = noneTileSprite.uv11Pixels;
                mesh.ChangeMeshArrayNode(x, y, gridUV00, gridUV11);

            }
        }
        mesh.UpdateMeshVals();
    }
    public TilemapSpriteUV[] TilemapSpriteUVArray { get { return tilemapSpriteUVArray; } }

    public Dictionary<Tilemap.TilemapObject.TilemapSprite, UVCoords> UVCoordsDictionary { get { return uvCoordsDictionary; } }

    public TilemapSpriteUV NoneTileSprite { get { return noneTileSprite; } set { noneTileSprite = value; } }

}

