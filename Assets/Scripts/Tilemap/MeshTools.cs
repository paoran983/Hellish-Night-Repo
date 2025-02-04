/*---------------------------------------------------------------------
 *  Class: MeshArray
 *
 *  Purpose: Creates and controls an array of quad (or square shaped) meshes
 *-------------------------------------------------------------------*/
using UnityEngine;

public class MeshArray
{
    public struct TilemapSpriteUV
    {
        public Tilemap.TilemapObject.TilemapSprite tilemapSprite;
        public Vector2Int uv00Pixels;
        public Vector2Int uv11Pixels;
    }

    private struct UVCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }
    private Mesh mesh;
    private Vector2[] uv;
    private Vector3[] vertices;
    private int[] triangles;
    private int gridSizeX, gridSizeY;
    private MeshFilter meshFilter;
    public MeshRenderer meshRender;

    public MeshArray(MeshFilter visualizer, MeshRenderer renderer)
    {

        this.meshFilter = visualizer;
        this.meshRender = renderer;
        UpdateLayer("UI", 0);
    }

    /*---------------------------------------------------------------------
     *  Method:CreateMeshGrid(int width, int height, float nodeSize)
     *
     *  Purpose: Creates an array of quad (or square shaped) meshes
     *           of the size width,height, with each quad having the width and length of 
     *           nodeSize
     *           
     *  Parameters: int width = x size of the mesh array
     *              int height = y size of the mesh array 
     *              float nodeSize = size of quads of the mesh
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void CreateMeshGrid(int width, int height, float nodeSize, float offset)
    {
        mesh = new Mesh();
        gridSizeX = Mathf.RoundToInt(width / nodeSize);
        gridSizeY = Mathf.RoundToInt(height / nodeSize);
        vertices = new Vector3[4 * gridSizeX * gridSizeY];
        uv = new Vector2[4 * gridSizeX * gridSizeY];
        triangles = new int[6 * gridSizeX * gridSizeY];
        float originX = 0;
        float originY = 0;
        Vector3 worldBottomLeft = (Vector3.zero - Vector3.right * width / 2 - Vector3.up * height / 2);
        if (worldBottomLeft != null)
        {
            originX = worldBottomLeft.x;
            originY = worldBottomLeft.y;
        }
        originX += offset;
        //originY += offset;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                int index = x * gridSizeY + y;
                // creates vertices of quad i.e the framwork of the quad
                vertices[index * 4 + 0] = new Vector3((nodeSize * x) + originX, (nodeSize * y) + originY);
                vertices[index * 4 + 1] = new Vector3((nodeSize * x) + originX, (nodeSize * (y + 1)) + originY);
                vertices[index * 4 + 2] = new Vector3((nodeSize * (x + 1) + originX), (nodeSize * (y + 1)) + originY);
                vertices[index * 4 + 3] = new Vector3((nodeSize * (x + 1) + originX), (nodeSize * y) + originY);
                // creates the triangles of the quad i.e the mesh/ body of the quad 
                // makes 2 tringles (3 call for each trianlge) bc 2 triangles make a quad
                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;
                triangles[index * 6 + 3] = index * 4 + 0;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;
                // assigns sprite of quad
                uv[index * 4 + 0] = new Vector2(0, 0);
                uv[index * 4 + 1] = new Vector2(0, 1);
                uv[index * 4 + 2] = new Vector2(1, 1);
                uv[index * 4 + 3] = new Vector2(1, 0);

            }
        }
        UpdateLayer("UI", 4);
        UpdateMeshVals();
    }

    /*---------------------------------------------------------------------
     *  Method: ChangeMeshArrayNode(int x, int y, Vector2 visual)
     *
     *  Purpose: Changes the sprite of a specific quad in a meshArray
     *  
     *  Parameters: int x = the x coord of the quad to change
     *              int y = the y coord of the quad to change  
     *              Vector2 visual = the visual to change the quad to 
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ChangeMeshArrayNode(int x, int y, Vector2 uv00, Vector2 uv11)
    {

        int index = x * (int)gridSizeY + y;
        uv[index * 4 + 0] = uv00;
        uv[index * 4 + 1] = new Vector2(uv00.x, uv11.y);
        uv[index * 4 + 2] = uv11;
        uv[index * 4 + 3] = new Vector2(uv11.x, uv00.y);
        UpdateMeshVals();
        meshFilter.mesh = mesh;


    }
    /*---------------------------------------------------------------------
     *  Method: UpdateMeshVals()
     *
     *  Purpose: Updates the mesh by first clearing it and them re assigning the mesh values. 
     *           As well as recaultate the normals so the mesh reacts to lighting correctly in unity
     *           
     *  Parameters: int x = the x coord of the quad to change
     *              int y = the y coord of the quad to change  
     *              Vector2 visual = the visual to change the quad to 
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void UpdateMeshVals()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        UpdateLayer("UI", 4);
        // has unity fix issues with lighting
        //mesh.RecalculateNormals();
    }
    public void UpdateLayer(string name, int order)
    {
        if (meshRender != null)
        {
            //Debug.Log(" making layer " + name);
            //meshRender.sortingLayerName = name;
            //  meshRender.sortingLayerID = order;
            //  meshRender.sortingOrder = order;
        }
    }
    public Mesh Mesh { get { return mesh; } }

    public Vector2[] Uv
    {
        get { return uv; }
        set { uv = value; }
    }
    public Vector3[] Verticies
    {
        get { return vertices; }
        set { vertices = value; }
    }
    public int[] Triangles
    {
        get { return triangles; }
        set { triangles = value; }
    }

}
