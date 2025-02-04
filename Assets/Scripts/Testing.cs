using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using CodeMonkey.Utils;
//using CodeMonkey;

public class Testing : MonoBehaviour {

    [SerializeField] private TilemapVisual tilemapVisual;
    private Tilemap tilemap;
    private Tilemap.TilemapObject.TilemapSprite tilemapSprite;
   // private TilemapVisual tilemapVisual;
  //  [SerializeField] private float nodeDiameter;
    private float worldSizeX,worldSizeY;
    [SerializeField] private GameObject world;
    private Mesh mesh;
    private Vector2[] uv;
    private Vector3[] vertices;
    private int[] triangles;
    private void Start() {
    
        worldSizeX = world.GetComponent<RectTransform>().rect.width;
        worldSizeY = world.GetComponent<RectTransform>().rect.height;
    //   tilemap = new Tilemap((int)worldSizeX,(int) worldSizeY, 0.5f);


     //   tilemap.SetTilemapVisual(tilemapVisual);
    }

    public void CreateMeshGrid(int width, int height, int nodeSize) {
        mesh = new Mesh();

        vertices = new Vector3[4* width*height];
        uv = new Vector2[4 * width * height];
        triangles = new int[6* width*height];
        float originX = 0; 
        float originY = 0;
        Vector3 worldBottomLeft = Vector3.zero - Vector3.right * width / 2 - Vector3.up * height / 2;
        if (worldBottomLeft != null) {
            originX = worldBottomLeft.x;
            originY = worldBottomLeft.y;
        }

        for(int x =0;x< width; x++) {
            for(int y =0;y< height;y++) {
                int index = x * height + y;
                vertices[index * 4 + 0] = new Vector3((nodeSize * x)+ originX, (nodeSize*y) + originY);
                vertices[index * 4 + 1] = new Vector3((nodeSize * x) + originX, (nodeSize *(y+1)) + originY);
                vertices[index * 4 + 2] = new Vector3((nodeSize * (x+1) + originX), (nodeSize * (y+1)) + originY);
                vertices[index * 4 + 3] = new Vector3((nodeSize * (x+1) + originX), (nodeSize * y) + originY);

                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;
                triangles[index * 6 + 3] = index * 4 + 0;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                uv[index * 4 + 0] = new Vector2(1, 1);
                uv[index * 4 + 1] = new Vector2(1, 1);
                uv[index * 4 + 2] = new Vector2(1, 1);
                uv[index * 4 + 3] = new Vector2(1, 1);

            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public void ChangeMeshArrayNode(int x,int y,Vector2 visual) {
        int index = x*(int)worldSizeY+y;
        uv[index * 4 + 0] = visual;
        uv[index * 4 + 1] = visual;
        uv[index * 4 + 2] = visual;
        uv[index * 4 + 3] = visual;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
          // tilemap.SetTilemapSprite(mouseWorldPosition, tilemapSprite);
        }
        if (Input.GetMouseButton(1)) {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           // tilemap.ClearTileMap(mouseWorldPosition);
        }
        /*
        
        if (Input.GetKeyDown(KeyCode.T)) {
            tilemapSprite = Tilemap.TilemapObject.TilemapSprite.None;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Ground;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Path;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            tilemapSprite = Tilemap.TilemapObject.TilemapSprite.Dirt;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }

        
        if (Input.GetKeyDown(KeyCode.P)) {
            tilemap.Save();
            CMDebug.TextPopupMouse("Saved!");
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            tilemap.Load();
            CMDebug.TextPopupMouse("Loaded!");
        }*/
    }

}
