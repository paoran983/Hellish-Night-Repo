/*---------------------------------------------------------------------
 *  Class: CombatGrid
 *
 *  Purpose: Control the underlying grid of nodes that controll 
 *           and aloow for the movement of units 
 *-------------------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrid : MonoBehaviour
{
    private Grid<Node> grid;
    [SerializeField] private TilemapVisual tilemapVisual;
    [SerializeField] private float nodeRadius, nodeDiameter, detectorOffest;
    private float worldSizeX, worldSizeY;
    [SerializeField] private List<LayerMask> unwalkalbeMasks;
    [SerializeField] private List<Tile.Type> unwalkalbeTypes;
    [SerializeField] private int gridSizeX, gridSizeY;
    [SerializeField] private GameObject world;
    private Tilemap tilemap;
    private Vector3 worldBottomLeft;
    private List<Node> path;
    [SerializeField] float offset;
    public Node moveDest;
    public Node curDest;
    public float leftAngle, leftAngle2;
    public int debugLevel;
    private TextMesh[,] debugText;
    public void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        path = new List<Node>();
        worldSizeX = world.GetComponent<RectTransform>().rect.width;
        worldSizeY = world.GetComponent<RectTransform>().rect.height;
        gridSizeX = Mathf.RoundToInt(worldSizeX / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(worldSizeY / nodeDiameter);
        CreateGrid();
        tilemap = new Tilemap((int)worldSizeX, (int)worldSizeY, nodeRadius);
        tilemap.SetTilemapVisual(tilemapVisual);
    }


    /*---------------------------------------------------------------------
     *  Method: CreateGrid()
     *
     *  Purpose: creates the underlying grid of nodes
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void CreateGrid()
    {
        grid = new Grid<Node>(worldSizeX, worldSizeY, nodeRadius, () => new Node());
        worldBottomLeft = Vector3.zero - Vector3.right * (worldSizeX) / 2 - Vector3.up * (worldSizeY) / 2;
        int count = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // gets each point a node will occupy in the world/scene
                Vector3 worldPoint = worldBottomLeft +
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.up * (y * nodeDiameter + nodeRadius);
                // determines if node is walkable by
                // checking if node is on top of any obstacles
                Vector2 box = new Vector2(nodeRadius + detectorOffest, nodeRadius + detectorOffest);
                bool walkable2 = true;
                int layer = 0;
                Tile.Type type = default;
                Tile tile = null;
                Collider2D[] tileColliders = null;
                SerializableDictionary<int, Tile.Type> levels = new SerializableDictionary<int, Tile.Type>();
                SerializableDictionary<int, Tile> tileLevels = new SerializableDictionary<int, Tile>();
                SerializableDictionary<int, bool> transactionStart = new SerializableDictionary<int, bool>();
                SerializableDictionary<int, bool> transactionEnd = new SerializableDictionary<int, bool>();
                SerializableDictionary<int, bool> stairRail = new SerializableDictionary<int, bool>();
                SerializableDictionary<int, bool> blocked = new SerializableDictionary<int, bool>();
                // sets node witl tile data

                tileColliders = Physics2D.OverlapBoxAll(worldPoint, box, 90);
                foreach (Collider2D tileCollider in tileColliders)
                    if (tileCollider != null)
                    {
                        if (tileCollider.gameObject != null)
                        {
                            tile = tileCollider.gameObject.GetComponent<Tile>();
                            if (tile != null)
                            {
                                layer = tile.gameObject.layer;
                                type = tile.type;

                                if (type == Tile.Type.stiars)
                                {
                                    levels.AddCheck(tile.endLayerInt, type);
                                    levels.AddCheck(tile.startLayerInt, type);
                                    tileLevels.AddCheck(tile.endLayerInt, tile);
                                    tileLevels.AddCheck(tile.startLayerInt, tile);
                                    if (tileCollider == tile.start)
                                    {
                                        transactionStart.AddCheck(tile.startLayerInt, true);
                                        transactionStart.AddCheck(tile.endLayerInt, true);
                                    }
                                    else if (tileCollider == tile.end)
                                    {
                                        transactionEnd.AddCheck(tile.endLayerInt, true);
                                        transactionEnd.AddCheck(tile.startLayerInt, true);
                                    }
                                    else if (tileCollider == tile.stairRails)
                                    {
                                        blocked.AddCheck(tile.startLayerInt, true);
                                        blocked.AddCheck(tile.endLayerInt, true);
                                    }
                                }

                                else
                                {
                                    tileLevels.AddCheck(layer, tile);
                                    levels.AddCheck(layer, type);
                                }


                                if (unwalkalbeTypes.Contains(type))
                                {
                                    blocked.AddCheck(layer, true);
                                }
                                else
                                {
                                    blocked.AddCheck(layer, false);
                                }
                            }
                        }
                    }
                // adds node to grid
                count++;
                grid.SetGridObject(x, y, new Node(walkable2, worldPoint, layer, type, levels, tileLevels,
                    transactionStart, transactionEnd, stairRail, blocked, x, y));
            }

        }
    }
    /*---------------------------------------------------------------------
    *  Method: isEnemy(Unit curUnit)
    *
    *  Purpose: Corutine that follows the path of a unit by going throught the 
    *           list and moving the unit to each waypoint
    *             
    *  Parameters: none
    *  Returns: none
    *-------------------------------------------------------------------*/
    public IEnumerator FollowPath(List<Node> path, GameObject toMove, Action toComplete)
    {
        Vector3 curWaypoint = new Vector3(path[0].WorldPos.x, path[0].WorldPos.y, path[0].WorldPos.z);
        Vector3 nextWaypoint = new Vector3(path[0].WorldPos.x, path[0].WorldPos.y, path[0].WorldPos.z);
        //Node curNode = grid.NodeFromWorldPoint(transform.position + nodeOffest);
        Node curWayPointNode = grid.NodeFromWorldPoint(curWaypoint);
        Node nextWayPointNode = grid.NodeFromWorldPoint(curWaypoint);
        int targetIndex = 0;
        if (toMove != null)
        {

            while (true)
            {

                // if waypoint is reached

                if (toMove.transform.position == curWaypoint)
                {
                    targetIndex++;

                    // if unit has reached destination or has collided with target
                    if (targetIndex >= path.Count)
                    {
                        if (toComplete != null)
                        {
                            toComplete();
                        }
                        yield break;

                    }
                    // gets next waypoint to follow
                    curWaypoint = new Vector3(path[targetIndex].WorldPos.x, path[targetIndex].WorldPos.y, toMove.transform.position.z);
                    // checks if next wayPoint is a unit if it is it stops path
                    curWayPointNode = grid.NodeFromWorldPoint(curWaypoint);

                }

                // moves unit twords curWayPoint
                toMove.transform.position = Vector3.MoveTowards(toMove.transform.position, curWaypoint, 20 * Time.deltaTime);

                if (toComplete != null)
                {
                    toComplete();
                }
                yield return null;

            }
        }
        if (toComplete != null)
        {
            toComplete();
        }
    }
    /*---------------------------------------------------------------------
     *  Method:  NodeFromWorldPoint(Vector3 worldPos)
     *
     *  Purpose: Gets the nodes at a specific location
     *             
     *  Parameters: Vector3 worldPos = location of the node to find
     *  Returns:  Node = the possible node at worldPos or null if not found
     *-------------------------------------------------------------------*/
    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        if (grid != null)
        {
            return grid.NodeFromWorldPoint(worldPos);
        }
        return null;
    }

    /*---------------------------------------------------------------------
     *  Method: ReachedDestination(Vector3 start,Vector3 target)
     *
     *  Purpose: Returns true if start is on the same node as target
     *             
     *  Parameters: Vector3 start = the starting location 
     *              Vector3 target = the target location
     *  Returns: true if start is on the same node as target
     *           and false if not
     *-------------------------------------------------------------------*/
    public bool ReachedDestination(Vector3 start, Vector3 target)
    {
        Node startNode = NodeFromWorldPoint(start);
        Node targetNode = NodeFromWorldPoint(target);
        if (startNode == null || targetNode == null)
        {
            // Debug.Log("fail");
            return false;
        }
        if (startNode.WorldPos == targetNode.WorldPos)
        {
            //  Debug.Log("done !!!");
            return true;
        }

        return false;
    }

    /*---------------------------------------------------------------------
     *  Method:  Node GetNodeAt(int x, int y)
     *
     *  Purpose: Gets the nodes at coords [x,y]
     *             
     *  Parameters: int x = the x coord of the node to find
     *              int y = the y coord of the node to find
     *  Returns:  Node = the possible node at coords [x,y] or null if not found
     *-------------------------------------------------------------------*/
    public Node GetNodeAt(int x, int y)
    {
        if (x < worldSizeX && y < worldSizeY)
        {
            return grid.GetGridObject(x, y);
        }
        return null;
    }
    public void ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite sprite, TilemapVisual tilemapVisual2, Tilemap tilemap2, Vector3 worldPos)
    {
        Tilemap.TilemapObject tilemapObject = tilemap2.NodeFromWorldPoint(worldPos);

        if (tilemapObject == null)
        {
            return;
        }
        if (sprite.Equals(tilemapObject.GetTilemapSprite())) { return; }
        // Debug.Log("changin from" + tilemapObject.GetLastTilemapSprite() + " to " + sprite);
        tilemapObject.SetTilemapSprite(sprite);
        int tilemapSpriteIndex = tilemapVisual2.GetTilemapSpriteUVIndex(sprite);
        if (tilemapSpriteIndex < 0)
        {
            return;
        }

        TilemapVisual.UVCoords spriteCorrds = tilemapVisual2.GetUvSpriteCoord(sprite);
        //Debug.Log("  now  is" + tilemapObject.GetTilemapSprite() + " was " + tilemapObject.GetLastTilemapSprite() + " coords = " + spriteCorrds.uv00 + "  " + spriteCorrds.uv11);
        tilemap2.ChangeTilemapNode(tilemapObject.X, tilemapObject.Y, spriteCorrds.uv00, spriteCorrds.uv11);
    }
    public void ResetNode(Vector3 worldPos)
    {
        Tilemap.TilemapObject tilemapObject = tilemap.NodeFromWorldPoint(worldPos);

        if (tilemapObject == null)
        {
            return;
        }
        Tilemap.TilemapObject.TilemapSprite last = tilemapObject.GetLastTilemapSprite();
        ChangeTilemapSprite(last, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForRange(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.InRange, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForArrowUp(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.ArrowUp, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForArrowDwon(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.ArrowDown, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeArrowLeft(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.ArrowLeft, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForArrowRight(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.ArrowRight, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForDiagLeft(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.DiagLeft, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForDiagRight(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.DiagRight, tilemapVisual, tilemap, worldPos);
    }
    public void ActivatePathUpOrDown(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.UpOrDown, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForAttack(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.Attack, tilemapVisual, tilemap, worldPos);
    }
    public void SelectNode(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.Selected, tilemapVisual, tilemap, worldPos);
    }
    public void ActivateNodeForCurTurn(Vector3 worldPos)
    {
        ChangeTilemapSprite(Tilemap.TilemapObject.TilemapSprite.CurTurn, tilemapVisual, tilemap, worldPos);
    }

    public void ClearNode(Vector3 worldPos)
    {
        Tilemap.TilemapObject tilemapObject = tilemap.NodeFromWorldPoint(worldPos);

        if (tilemapObject == null)
        {
            return;
        }
        TilemapVisual.TilemapSpriteUV noneTile = tilemapVisual.NoneTileSprite;
        if (noneTile.Equals(null))
        {
            return;
        }
        tilemapObject.SetTilemapSprite(Tilemap.TilemapObject.TilemapSprite.None);
        tilemap.ChangeTilemapNode(tilemapObject.X, tilemapObject.Y, noneTile.uv00Pixels, noneTile.uv11Pixels);
    }

    public void ClearPathStep(Vector3 worldPos)
    {
        Tilemap.TilemapObject tilemapObject = tilemap.NodeFromWorldPoint(worldPos);

        if (tilemapObject == null)
        {
            return;
        }
        TilemapVisual.TilemapSpriteUV noneTile = tilemapVisual.NoneTileSprite;
        if (noneTile.Equals(null))
        {
            return;
        }
        tilemap.ChangeTilemapNode(tilemapObject.X, tilemapObject.Y, noneTile.uv00Pixels, noneTile.uv11Pixels);
    }
    public List<Node> GetPushPath(Node pos1, Node pos2, int k)
    {
        int dx = pos2.GridX - pos1.GridX;
        int dy = pos2.GridY - pos1.GridY;

        // Determine the primary direction steps
        int stepX = dx == 0 ? 0 : dx / Math.Abs(dx); // Horizontal step direction
        int stepY = dy == 0 ? 0 : dy / Math.Abs(dy); // Vertical step direction
        List<Node> lineCells = new List<Node>();
        Node currentPos = pos1;
        int totalX = stepX;
        int totalY = stepY;
        // First, move from pos1 to pos2 using stair-step approach
        while (currentPos != pos2)
        {
            // Move primarily in the x direction, then adjust y
            if (Math.Abs(pos2.GridX - currentPos.GridX) > (Math.Abs(pos2.GridY - currentPos.GridY)))
            {

                currentPos = grid.GetGridObject(currentPos.GridX + stepX, currentPos.GridY);
                totalX += stepX;

            }
            else if (Math.Abs(pos2.GridX - currentPos.GridX) == Math.Abs(pos2.GridY - currentPos.GridY))
            {
                currentPos = grid.GetGridObject(currentPos.GridX + stepX, currentPos.GridY + stepY);
                totalX += stepX;
                totalY += stepY;

            }
            else
            {
                currentPos = grid.GetGridObject(currentPos.GridX, currentPos.GridY + stepY);
                totalY += stepY;
            }

        }

        Node extendedTarget = grid.GetGridObject(pos2.GridX + totalX, pos2.GridY + totalY);
        if (extendedTarget != null)
        {
            int i = 0;
            foreach (Node node in GetLine(pos2, extendedTarget))
            {
                if (i > k) break;
                if ((node.Walkable == false || node.Unit != null) && (node != pos2 && node != extendedTarget))
                {
                    break;
                }
                lineCells.Add(node);
                i++;
            }
        }

        return lineCells;
    }
    public Node GetPushDestination(Node pos1, Node pos2, int k)
    {
        List<Node> lineNodes = GetPushPath(pos1, pos2, k);
        Node extenedTarget = null;
        if (lineNodes != null)
        {
            extenedTarget = lineNodes[lineNodes.Count - 1];
        }

        return extenedTarget;
    }

    public List<Node> GetLine(Node start, Node target)
    {
        List<Node> res = new List<Node>();
        if (start == null || target == null) return res;
        int x1 = start.GridX;
        int y1 = start.GridY;
        int x2 = target.GridX;
        int y2 = target.GridY;
        List<Node> linePoints = new List<Node>();
        // gets diffetence between target and start
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        // gets step direction of line or diretion line is moving in
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;

        int err = (dx - dy);
        int steps = Math.Max(dx, dy); // Determine the number of steps for the loop

        for (int i = 0; i <= steps; i++) // Loop through each step
        {

            Node step = grid.GetGridObject(x1, y1);
            linePoints.Add(step);
            if (x1 == x2 && y1 == y2)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }

            if (e2 < dx + 1)
            {
                err += dx;
                y1 += sy;
            }
        }

        return linePoints;
    }
    public bool IsLineBlocked(Node start, Node target, int startLayer, int targetLayer)
    {
        List<Node> line = GetLine(start, target);
        for (int i = 0; i < line.Count - 3; i++)
        {
            Node node = line[i];
            if (node != null)
            {
                if (node.isBlocked(startLayer) || node.isBlocked(targetLayer))
                {
                    return true;
                }

            }
        }


        return false;
    }

    /*---------------------------------------------------------------------
     *  Method:  GetFilledCells(HashSet<Node> border)
     *
     *  Purpose: Takes a set of nodes thatmake a closed shape and returns a 
     *           a HashSet of nodes taht maek up the nodes that fill that shape. 
     *           This is done by doing through each node and making lines going up, 
     *           donw, left and right and only adds lines that hit another node 
     *           in the border or a node already in the res
     *             
     *  Parameters: HashSet<Node> border = set of noes that make a closed shape
     *  Returns:  HashSet<Node> res = nodes that fill in the shape made by border
     *-------------------------------------------------------------------*/
    public HashSet<Node> GetFilledCells(HashSet<Node> border)
    {

        HashSet<Node> res = new HashSet<Node>();
        if (border == null) return res;
        // find length and height of shape
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        int height = 0, width = 0;
        foreach (Node node in border)
        {
            foreach (Node node2 in border)
            {
                if (Math.Abs(node2.GridX - node.GridX) > width)
                {
                    width = Math.Abs(node2.GridX - node.GridX);
                }
                if (Math.Abs(node2.GridY - node.GridY) > height)
                {
                    height = Math.Abs(node2.GridY - node.GridY);
                }
            }
        }
        foreach (var coord in border)
        {
            minX = Math.Min(minX, coord.GridX);
            maxX = Math.Max(maxX, coord.GridX);
            minY = Math.Min(minY, coord.GridY);
            maxY = Math.Max(maxY, coord.GridY);
        }
        foreach (Node node in border)
        {

            foreach (Node fillNode in fillRightLeftHelper(width, node, border, minX, maxX, 0))
            {
                res.Add(fillNode);
            }
            foreach (Node fillNode in fillRightLeftHelper(width, node, border, minX, maxX, 1))
            {
                res.Add(fillNode);
            }
            foreach (Node fillNode in fillUpDownHelper(height, node, border, minY, maxY, 0))
            {
                res.Add(fillNode);
            }
            foreach (Node fillNode in fillUpDownHelper(height, node, border, minY, maxY, 1))
            {
                res.Add(fillNode);
            }

        }

        return res;
    }

    /*---------------------------------------------------------------------
     *  Method:  fillRightLeftHelper(int width, Node node, HashSet<Node> border, int minX, int maxX, int type)
     *
     *  Purpose: Helps fill that a shape by going through each node and making 
     *           lines going left and right and only adds lines that hit another node 
     *           in the border or a node already in the res
     *             
     *  Parameters: int width = width of the shpae
     *              Node node = the starting node of the shape
     *              HashSet<Node> border = the border of the shape
     *              int minX = smallest x val
     *              int maxX = largetst x val
     *              int type = 0 is right and 1 if left
     *  Returns:  HashSet<Node> res = nodes that fill in the shape made by border
     *-------------------------------------------------------------------*/
    public HashSet<Node> fillRightLeftHelper(int width, Node node, HashSet<Node> border, int minX, int maxX, int type)
    {
        HashSet<Node> res = new HashSet<Node>();
        //  Debug.Log(" GETTING LEFT/RIGHT---------------");
        for (int i = 1; i < width + 1; i++)
        {

            Node fillNodeRight = grid.GetGridObject(node.GridX + i, node.GridY);

            if (fillNodeRight != null && type == 0)
            {

                if (border.Contains(fillNodeRight) || res.Contains(fillNodeRight))
                {
                    return res;
                }
                res.Add(fillNodeRight);

            }
            Node fillNodeLeftt = grid.GetGridObject(node.GridX - i, node.GridY);
            if (fillNodeLeftt != null && type == 1)
            {
                //  Debug.Log(">>> found fill node at " + fillNodeLeftt.GridX + "," + fillNodeLeftt.GridY);

                if (border.Contains(fillNodeLeftt) || res.Contains(fillNodeLeftt))
                {
                    return res;
                }
                res.Add(fillNodeLeftt);

            }
        }
        return new HashSet<Node>();
    }
    /*---------------------------------------------------------------------
     *  Method:  fillUpDownHelper(int height, Node node, HashSet<Node> border, int minY, int maxY, int type)
     *
     *  Purpose: Helps fill that a shape by going through each node and making 
     *           lines going up and down and only adds lines that hit another node 
     *           in the border or a node already in the res
     *             
     *  Parameters: int height = height of the shpae
     *              Node node = the starting node of the shape
     *              HashSet<Node> border = the border of the shape
     *              int minX = smallest x val
     *              int maxX = largetst x val
     *              int type = 0 is up and 1 if down
     *  Returns:  HashSet<Node> res = nodes that fill in the shape made by border
     *-------------------------------------------------------------------*/
    public HashSet<Node> fillUpDownHelper(int height, Node node, HashSet<Node> border, int minY, int maxY, int type)
    {
        HashSet<Node> res = new HashSet<Node>();
        for (int i = 1; i < height + 1; i++)
        {
            Node fillNodeUp = grid.GetGridObject(node.GridX, node.GridY + i);
            if (fillNodeUp != null && type == 0)
            {

                if (border.Contains(fillNodeUp) || res.Contains(fillNodeUp))
                {
                    return res;
                }
                res.Add(fillNodeUp);
            }
            Node fillNodeDown = grid.GetGridObject(node.GridX, node.GridY - i);
            if (fillNodeDown != null && type == 1)
            {

                if (border.Contains(fillNodeDown) || res.Contains(fillNodeDown))
                {
                    return res;
                }
                res.Add(fillNodeDown);

            }
        }
        return new HashSet<Node>();
    }
    /*---------------------------------------------------------------------
     *  Method:  GetFilledCells(HashSet<Node> border)
     *
     *  Purpose: Takes a set of nodes and creates a closed shape using 
     *           each node as a vertecie and returns a 
     *           a HashSet of nodes taht make up the nodes that make up the shape shape. 
     *           This is done by making a striaght line between each 
     *           vertice to make a border and then doing through each 
     *           node and making lines going up, 
     *           donw, left and right and only adds lines that hit another node 
     *           in the border or a node already in the res
     *             
     *  Parameters: List<Node> vertices = ther vertices of the shape
     *              Node startNode = the sorce of the shape
     *  Returns:  HashSet<Node> res = nodes that fill in the shape made by border
     *-------------------------------------------------------------------*/
    public HashSet<Node> GetShapeByVertex(List<Node> vertices, Node startNode)
    {
        HashSet<Node> res = new HashSet<Node>();
        List<Node> borderNodes = new List<Node>();
        Node lastNode = startNode;
        foreach (Node node in vertices)
        {
            if (node != null)
            {
                // res.Add(node);
            }
            if (node != null && lastNode != null)
            {
                if (vertices.Count > 1)
                {
                    //  Debug.Log(" getting line from " + lastNode.GridX + "," + lastNode.GridY + "  " + node.GridX + "," + node.GridY);
                }
                foreach (Node node2 in GetLine(lastNode, node))
                {
                    res.Add(node2);
                }
                foreach (Node node2 in GetLine(node, lastNode))
                {
                    res.Add(node2);
                }
                lastNode = node;
            }
        }
        // fills shape made by units if more than 2
        if (vertices.Count > 2)
        {
            foreach (Node node in GetFilledCells(res))
            {
                res.Add((Node)node);
            }
        }

        return res;
    }




    public HashSet<Node> GetAllValidLineNodes(Node sourceNode, int range)
    {
        HashSet<Node> res = new HashSet<Node>();
        List<Node> possibleTargets = new List<Node>();
        if (sourceNode == null) return res;
        Vector2 source = new Vector2(sourceNode.GridX, sourceNode.GridY);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, 0, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, 0, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, 0, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, 0, range);
        foreach (Node curTarget in possibleTargets)
        {
            foreach (Node node in GetValidLineNodes(sourceNode, curTarget, range))
            {
                if (node != null)
                {
                    res.Add(node);
                }
            }
        }
        return res;
    }
    public HashSet<Node> GetAllSpreadNodes(Node sourceNode, int range, int spreadRange)
    {
        HashSet<Node> res = new HashSet<Node>();
        List<Node> possibleTargets = new List<Node>();
        if (sourceNode == null) return res;
        Vector2 source = new Vector2(sourceNode.GridX, sourceNode.GridY);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, 0, range, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, 0, range * -1, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range, 0, range);
        GetAllValidLineNodesHelper(possibleTargets, sourceNode, source, range * -1, 0, range);
        foreach (Node curTarget in possibleTargets)
        {
            foreach (Node node in GetSpread(sourceNode, curTarget, range, spreadRange))
            {
                if (node != null)
                {
                    res.Add(node);
                }
            }
        }
        return res;
    }
    public void GetAllValidLineNodesHelper(List<Node> possibleTargets, Node sourceNode, Vector2 source, int x, int y, int range)
    {
        Node possibleTarget = null;
        if (sourceNode == null) { return; }
        possibleTarget = GetNodeAt((int)source.x + x, (int)source.y + y);
        if (possibleTarget != null)
        {

            possibleTargets.Add(possibleTarget);

        }
    }

    public Node GetCentroid(List<Unit> nodes)
    {
        Vector2 total = Vector2.zero;
        foreach (Unit node in nodes)
        {
            total.x += node.CurNode.WorldPos.x;
            total.y += node.CurNode.WorldPos.y;

        }
        total /= nodes.Count;
        return grid.NodeFromWorldPoint(total);
    }
    public HashSet<Node> GetValidLineNodesHash(Node sourceNode, Node targetNode, int range)
    {
        HashSet<Node> res = new HashSet<Node>();
        foreach (Node node in GetValidLineNodes(sourceNode, targetNode, range))
        {
            res.Add(node);
        }
        return res;
    }

    public List<Node> GetValidLineNodes(Node sourceNode, Node targetNode, int range)
    {
        HashSet<Node> res = new HashSet<Node>();
        List<Node> finalRes = new List<Node>();
        if (sourceNode == null || targetNode == null) return finalRes;
        Vector2 target = new Vector2(targetNode.GridX, targetNode.GridY);
        Vector2 source = new Vector2(sourceNode.GridX, sourceNode.GridY);
        int difX = (int)target.x - (int)source.x;
        int difY = (int)target.y - (int)source.y;
        int absX = Math.Abs(difX);
        int absY = Math.Abs(difY);
        Node curStep = null;
        int pathLength = 0;
        if (absX == 0 && difY == 0)
        {
            return finalRes;
        }
        // if diag
        else if (absX == absY)
        {
            pathLength = range;

            for (int i = 1; i <= pathLength; i++)
            {
                if (difX < 0 && difY > 0)
                {
                    curStep = GetNodeAt((int)source.x - i, (int)source.y + i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
                else if (difX > 0 && difY < 0)
                {
                    curStep = GetNodeAt((int)source.x + i, (int)source.y - i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
                else if (difX > 0 && difY > 0)
                {
                    curStep = GetNodeAt((int)source.x + i, (int)source.y + i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
                else if (difX < 0 && difY < 0)
                {
                    curStep = GetNodeAt((int)source.x - i, (int)source.y - i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
            }

        }
        // if up or down
        else if (difX == 0 && difY != 0)
        {
            pathLength = range;
            for (int i = 1; i <= pathLength; i++)
            {
                if (difY > 0)
                {
                    curStep = GetNodeAt((int)source.x, (int)source.y + i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
                else if (difY < 0)
                {
                    curStep = GetNodeAt((int)source.x, (int)source.y - i);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
            }

        }
        // if left or right
        else if (difX != 0 && difY == 0)
        {
            pathLength = range;
            for (int i = 1; i <= pathLength; i++)
            {
                if (difX > 0)
                {
                    curStep = GetNodeAt((int)source.x + i, (int)source.y);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
                else if (difX < 0)
                {
                    curStep = GetNodeAt((int)source.x - i, (int)source.y);
                    if (curStep != null)
                    {
                        res.Add(curStep);
                    }
                }
            }

        }
        foreach (Node node in res)
        {
            if (GetDistance(sourceNode, node) <= range)
            {
                finalRes.Add(node);
            }


        }
        return finalRes;
    }


    public HashSet<Node> GetSpread(Node sourceNode, Node targetNode, int range, int spreadRange)
    {
        HashSet<Node> res = new HashSet<Node>();
        if (sourceNode == null || targetNode == null) return res;
        Vector2 target = new Vector2(targetNode.GridX, targetNode.GridY);
        Vector2 source = new Vector2(sourceNode.GridX, sourceNode.GridY);

        int difX = 0;
        int difY = 0;
        if (((int)target.x - (int)source.x) > 0)
        {
            difX = 1;
        }
        else if (((int)target.x - (int)source.x) < 0)
        {
            difX = -1;
        }
        if (((int)target.y - (int)source.y) > 0)
        {
            difY = 1;
        }
        else if (((int)target.y - (int)source.y) < 0)
        {
            difY = -1;
        }
        int absX = Math.Abs(difX);
        int absY = Math.Abs(difY);
        float moveSpreadAngle = (spreadRange * (Mathf.PI / 180));
        float centerAngle = 0;
        int type = 0;
        Node startingNode = null;
        if (absX == 0 && difY == 0)
        {
            return res;
        }
        List<Node> centerLine = GetValidLineNodes(sourceNode, targetNode, range);
        if (centerLine.Count <= 0)
        {
            return res;
        }
        // diag up right
        if (absX == absY && difX > 0 && difY > 0)
        {
            centerAngle = ((Mathf.PI) / 4);
            startingNode = grid.GetGridObject(centerLine[0].GridX - 1, centerLine[0].GridY - 1);
            type = 2;
        }
        // diag up left
        else if (absX == absY && difX < 0 && difY > 0)
        {
            centerAngle = ((3 * Mathf.PI) / 4);
            startingNode = grid.GetGridObject(centerLine[0].GridX, centerLine[0].GridY - 1);
            type = 2;
        }
        // diag down right
        else if (absX == absY && difX > 0 && difY < 0)
        {
            centerAngle = ((7 * Mathf.PI) / 4);
            startingNode = grid.GetGridObject(centerLine[0].GridX - 1, centerLine[0].GridY);
            type = 2;
        }
        // diag down left
        else if (absX == absY && difX < 0 && difY < 0)
        {
            type = 2;
            centerAngle = ((5 * Mathf.PI) / 4);
            startingNode = grid.GetGridObject(centerLine[0].GridX, centerLine[0].GridY);
        }
        // up
        else if (difX == 0 && difY > 0)
        {
            type = 0;
            centerAngle = (Mathf.PI / 2);
            startingNode = centerLine[0];
        }
        // down
        else if (difX == 0 && difY < 0)
        {
            centerAngle = ((3 * Mathf.PI) / 2);
            startingNode = grid.GetGridObject(centerLine[0].GridX, centerLine[0].GridY);
            type = 0;
        }
        // left
        else if (difX < 0 && difY == 0)
        {
            centerAngle = (Mathf.PI);
            startingNode = centerLine[0];

            type = 1;
        }
        // right
        else if (difX > 0 && difY == 0)
        {
            centerAngle = 0;
            startingNode = centerLine[0];
            type = 1;
        }
        foreach (Node node in GetSpreadSide(startingNode, centerLine, centerAngle, moveSpreadAngle, difX, difY, type, range))
        {
            res.Add(node);
        }

        Debug.Log(" spread found " + res.Count);

        return res;
    }

    public HashSet<Node> GetSpreadSide(Node source, List<Node> centerLine, float centerAngle, float moveSpreadAngle, int difX, int difY, int type, int range)
    {
        HashSet<Node> curRes = new HashSet<Node>();
        int x0 = 0;
        int y0 = 0;
        int x1 = 0;
        int y1 = 0;
        int x2 = 0;
        int y2 = 0;
        float side2Angle = centerAngle - moveSpreadAngle;
        float side1Angle = centerAngle + moveSpreadAngle;

        List<Node> side1 = new List<Node>();
        List<Node> side2 = new List<Node>();
        int xDif = x2 - x1;
        int yDif = y2 - y1;
        int xAdder = 0;
        int yAdder = 0;
        if (centerLine.Count <= 0) return curRes;
        if (type == 0 || type == 1)
        {
            centerLine.RemoveAt(centerLine.Count - 1);
        }
        x0 = source.GridX;
        y0 = source.GridY;
        if (type == 0)
        {
            if (difY < 1)
            {
                y0 -= 1;
            }

        }
        else if (type == 1)
        {
            if (difX < 1)
            {
                x0 -= 1;
            }

        }

        curRes.Add(centerLine[0]);
        for (int i = 0; i <= centerLine.Count + 1; i++)
        {
            x1 = (int)Mathf.Ceil(x0 + (i + 1) * Mathf.Cos(side1Angle));
            y1 = (int)Mathf.Ceil(y0 + (i + 1) * Mathf.Sin(side1Angle));
            x2 = (int)Mathf.Ceil(x0 + (i + 1) * Mathf.Cos(side2Angle));
            y2 = (int)Mathf.Ceil(y0 + (i + 1) * Mathf.Sin(side2Angle));
            if (type == 0)
            {
                if (difY < 1)
                {
                    y2 -= difY;
                }
                else
                {
                    y1 += difY;
                }
            }
            else if (type == 1)
            {
                if (difX >= 1)
                {
                    x2 -= difX;
                }
                else
                {
                    x1 += difX;
                }
            }
            xDif = x2 - x1;
            yDif = y2 - y1;
            xAdder = 0;
            yAdder = 0;
            if (xDif > 0 && type != 1)
            {
                xAdder = 1;
            }
            else if (xDif < 0 && type != 1)
            {
                xAdder = -1;
            }
            if (yDif > 0 && type != 0)
            {
                yAdder = 1;
            }
            else if (yDif < 0 && type != 0)
            {
                yAdder = -1;
            }
            Node node = grid.GetGridObject(x1, y1);
            Node nodeOp = grid.GetGridObject(x2, y2);
            Node diagNode = null;
            Node diagNodeOp = null;
            // curRes.Add(nodeOp);
            // curRes.Add(node);
            if (GetDistance(nodeOp, source) < range)
            {

                curRes.Add(nodeOp);
            }
            if (GetDistance(node, source) < range)
            {

                curRes.Add(node);
            }
            side2.Add(nodeOp);
            side1.Add(node);
        }

        FillInSpread(source, side1, side2, curRes, xAdder, yAdder, type, range);
        foreach (Node node2 in centerLine)
        {

            curRes.Add(node2);

        }
        return curRes;

    }

    public void FillInSpread(Node source, List<Node> side1, List<Node> side2, HashSet<Node> res, int xAdder, int yAdder, int type, int range)
    {
        if (side1 == null || side2 == null || res == null) return;
        int length = Mathf.Min(side1.Count, side2.Count);
        HashSet<Node> tmpRes = new HashSet<Node>();
        for (int i = 0; i < length; i++)
        {
            Node node = side1[i];
            Node nodeOp = side2[i];
            int distance = GetDistance(node, nodeOp);
            for (int j = 1; j < distance; j++)
            {
                // has lines that grow from each side until they meet
                Node curNode = grid.GetGridObject(nodeOp.GridX - (j * xAdder), nodeOp.GridY - (j * yAdder));
                Node curNode2 = grid.GetGridObject(node.GridX + (j * xAdder), node.GridY + (j * yAdder));
                if (curNode2 != null)
                {
                    tmpRes.Add(curNode2);
                }
                if (curNode != null)
                {
                    if (res.Contains(curNode)) break;
                    tmpRes.Add(curNode);
                    // fills in missing sports for diag
                    if (type >= 2)
                    {
                        Node diag1 = grid.GetGridObject(curNode.GridX, curNode.GridY - yAdder);
                        if (diag1 != null)
                        {
                            if (diag1 != null)
                            {
                                tmpRes.Add(diag1);
                            }
                        }
                        Node diag2 = grid.GetGridObject(curNode.GridX + xAdder, curNode.GridY);
                        if (diag2 != null)
                        {
                            if (diag2 != null)
                            {
                                tmpRes.Add(diag2);
                            }
                        }
                    }
                }
            }
        }
        foreach (Node curNode in tmpRes)
        {
            if (GetDistance(curNode, source) < range)
            {

                res.Add(curNode);
            }
        }


    }
    /*---------------------------------------------------------------------
     *  Method:  GetNeighbors(Node node)
     *
     *  Purpose: Gets the nodes surrounding a certain node
     *             
     *  Parameters: Node node = the node to find neightbors of
     *  Returns:  List<Node> neighbors = the list of neightbors nodes
     *-------------------------------------------------------------------*/
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        // searchs around node
        for (int x = -(int)nodeDiameter; x <= (int)nodeDiameter; x++)
        {
            for (int y = -(int)nodeDiameter; y <= (int)nodeDiameter; y++)
            {
                // skips the center node
                if (x == 0 && y == 0)
                {
                    continue;
                }
                // skips diag
                /*if (Mathf.Abs(x) == Mathf.Abs(y)) {
                    continue;
                }*/
                int checkX = node.GridX + x;
                int checkY = node.GridY + y;
                // checks if within bounds
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    // adds valid neighbor
                    neighbors.Add(grid.GetGridObject(checkX, checkY));
                }
            }
        }
        return neighbors;
    }

    /*---------------------------------------------------------------------
     *  Method:  GetNeighborsInRange(Node node, int maxDistance)
     *
     *  Purpose: Gets the nodes within the range of a units maxMoveDistance
     *             
     *  Parameters: Node node = the node to find neightbors of
     *              int maxDistance = the rnage to find nodes within
     *  Returns:  List<Node> neighbors = the list of neightbors nodes
     *-------------------------------------------------------------------*/
    public HashSet<Node> GetNeighborsInRange(Node node, int maxDistance)
    {
        List<Node> neighbors = new List<Node>();
        HashSet<Node> res = new HashSet<Node>();
        // searchs around node
        Node curNode = node;
        int j = 0;
        int counter = 0;
        int max = 0;
        // gets the distance of the furthest possible node that is still in range
        for (int i = 1; i <= maxDistance; i++)
        {
            max += 8 * i;
        }
        for (int i = 0; i <= maxDistance; i++)
        {
            counter = 0;
            // gets a maxDistance amount of layers around curNode
            while (counter <= (8 * i))
            {
                // checks the nodes surrounding curNode
                for (int x = -(int)nodeDiameter; x <= (int)nodeDiameter; x++)
                {
                    for (int y = -(int)nodeDiameter; y <= (int)nodeDiameter; y++)
                    {

                        // skips the center node
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                        int checkX = curNode.GridX - x;
                        int checkY = curNode.GridY + y;
                        // checks if within bounds
                        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                        {

                            // adds valid neighbor
                            Node temp = grid.GetGridObject(checkX, checkY);
                            if (!neighbors.Contains(temp) && temp.WorldPos != node.WorldPos)
                            {
                                neighbors.Add(temp);
                                if (GetDistance(node, temp) <= maxDistance)
                                {
                                    res.Add(temp);
                                }
                            }

                        }
                    }
                }
                if (j >= max)
                {
                    break;
                }

                curNode = neighbors[j];
                j++;
                counter++;

            }
        }
        return res;
    }
    public int GetDistance(Node startNode, Node targetNode)
    {
        int distX = Mathf.Abs(startNode.GridX - targetNode.GridX);
        int distY = Mathf.Abs(startNode.GridY - targetNode.GridY);
        int distZ = Mathf.Abs(startNode.Height - targetNode.Height);

        int distance = Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(distX, 2) + Mathf.Pow(distY, 2) + Mathf.Pow(distZ, 2)));
        return distance;


    }
    /*---------------------------------------------------------------------
     *  Method:  ChangeUnitPos(Vector3 origin, Vector3 destination)
     *
     *  Purpose: Moves a unit on the underlying grid
     *             
     *  Parameters: Vector3 origin = units initial location
     *              Vector3 destination = units current location
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ChangeUnitPos(Vector3 origin, Vector3 destination)
    {
        if (origin == destination)
        {
            return;
        }
        Node startNode = NodeFromWorldPoint(origin);
        Node endNode = NodeFromWorldPoint(destination);
        Unit tempUnit = endNode.Unit;
        endNode.Unit = startNode.Unit;
        startNode.Unit = tempUnit;
        bool temp = endNode.Walkable;
        endNode.Walkable = startNode.Walkable;
        startNode.Walkable = temp;
    }
    /*---------------------------------------------------------------------
    *  Method:  ChangeUnitNodePos(Vector3 origin, Vector3 destination)
    *
    *  Purpose: Moves a unit on the underlying grid based on thier node
    *             
    *  Parameters: Vector3 origin = units initial location
    *              Vector3 destination = units current location
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void ChangeUnitNodePos(Node startNode, Node endNode)
    {
        if (startNode == null || endNode == null)
        {
            return;
        }
        Unit tempUnit = endNode.Unit;
        endNode.Unit = startNode.Unit;
        startNode.Unit = tempUnit;
        bool temp = endNode.Walkable;
        endNode.Walkable = startNode.Walkable;
        startNode.Walkable = temp;

    }
    void OnDrawGizmos()
    {
        bool debug = true;
        if (debug)
        {
            //Debug.Log("grid");
            //Gizmos.DrawCube(transform.position, new Vector3(worldSize.x, worldSize.y, 1));
            if (grid != null)
            {
                // Debug.Log("grid");
                //Node playerNode = NodeFromWorldPoint(player.position);
                foreach (Node node in grid.GridArray)
                {
                    //     Debug.Log("node "+ node.Walkable);
                    //Gizmos.color = (node.Walkable) ? Color.white : Color.red;
                    if (path != null)
                    {
                        //Debug.Log("path");

                        if (path.Contains(node) && path.IndexOf(node) == 0)
                        {
                            Gizmos.color = Color.yellow;
                        }
                        if (path.Contains(node) && path.IndexOf(node) == path.Count - 1)
                        {
                            Gizmos.color = Color.cyan;
                        }

                    }
                    if (node.InRange)
                    {
                        Gizmos.color = Color.green;
                    }
                    if (node == moveDest)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    if (node == curDest)
                    {
                        Gizmos.color = Color.clear;
                    }
                    if (node.GetLevelType(debugLevel) == Tile.Type.floor)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    if (node.GetLevelType(debugLevel) == Tile.Type.deafult)
                    {
                        Gizmos.color = Color.white;
                    }
                    if (node.GetLevelType(debugLevel) == Tile.Type.building)
                    {
                        Gizmos.color = Color.grey;
                    }
                    if (node.GetLevelType(debugLevel) == Tile.Type.stiars)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    if (node.isTransEnd(debugLevel))
                    {
                        Gizmos.color = Color.blue;
                    }
                    if (node.isTransStart(debugLevel))
                    {
                        Gizmos.color = Color.cyan;
                    }
                    if (node.isRail(debugLevel))
                    {
                        Gizmos.color = Color.magenta;
                    }
                    if (node.isBlocked(debugLevel))
                    {
                        // Gizmos.DrawSphere(node.WorldPos, (0.25f));
                        Gizmos.color = Color.red;
                    }



                    if (node.Unit != null)
                    {
                        if (node.Unit.Team == 0)
                        {
                            // Gizmos.color = Color.blue;
                        }
                        else if (node.Unit.Team == 1)
                        {
                            //   Gizmos.color = Color.magenta;
                        }
                        //Gizmos.color = Color.magenta;
                    }
                    //Gizmos.color = (node.Walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(node.WorldPos, Vector3.one * (nodeDiameter - .1f));

                    //  Gizmos.DrawIcon(node.WorldPos, node.LayerInt.ToString(), true);
                    float size = 0;
                    if (node.LayerInt % 2 == 0)
                    {
                        size = (float)(node.LayerInt);
                    }
                    else
                    {
                        size = (float)(node.LayerInt * 10);
                    }


                }
            }

        }
    }
    public Vector2 Box { get { return new Vector2(nodeRadius + detectorOffest, nodeRadius + detectorOffest); } }

    public List<Node> Path
    {
        get { return this.path; }
        set { this.path = value; }
    }
    public int GridSizeX
    {
        get
        {
            return gridSizeX;
        }
    }
    public int GridSizeY
    {
        get
        {
            return gridSizeY;
        }
    }

    public Tilemap TileMap
    {
        get
        {
            return tilemap;
        }
    }
    public int MaxSize()
    {
        return gridSizeX * gridSizeY;
    }
    public Vector3 WorldBottomLeft
    {
        get
        {
            return worldBottomLeft;
        }
        set
        {
            worldBottomLeft = value;
        }
    }



}
