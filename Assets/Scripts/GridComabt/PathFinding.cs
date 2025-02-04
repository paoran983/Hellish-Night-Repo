
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public CombatGrid grid;
    private Transform seeker;
    private Transform target;
    // [SerializeField] private PathRequestManager pathRequestManager;
    void Awake()
    {

        grid = GetComponent<CombatGrid>();
        //   pathRequestManager = GetComponent<PathRequestManager>();


    }
    public void FindPath(PathRequest request, Action<PathResult> callback)
    {

        //  Debug.Log("finding path");
        Vector3[] wayPoints = new Vector3[0];
        Vector3[] dirPath = new Vector3[0];
        PathAndDir pathAndDir = null;
        bool pathSuccess = false;
        Node startNode = grid.NodeFromWorldPoint(request.PathStart);

        //Node startNodeFeet = request.Sender.CurNode;
        //startNodeFeet.Walkable = true;
        int pathLength = 0;
        Node targetNode = grid.NodeFromWorldPoint(request.PathEnd);



        startNode.Parent = startNode;
        int curLevel = request.Sender.gameObject.layer;
        int tempLevel = curLevel;
        if (request.Debug)
        {
            Debug.Log("---**************--------start-----***************---------- at level " + curLevel);

            Debug.Log("  ========= starting at  " + startNode.GridX + " " + startNode.GridX + " and going to " + targetNode.GridX + " " + targetNode.GridX);
        }
        // Debug.Log("-----------start--------------- at level " + curLevel);
        // checks if path is possible

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize());
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // find Node with openSet with the lowest f cost
            // that is the smallest distace from the target
            //  && openSet[i].GetHCost() < curNode.GetHCost())
            Node curNode = openSet.RemoveFirst();
            closedSet.Add(curNode);
            // if target is found and path finding is done
            if (curNode == targetNode)
            {
                if (request.Debug)
                {
                    Debug.Log("----============-------found----==============----------- ");
                }
                // print("-----------found--------------- ");
                pathSuccess = true;
                break;
            }
            //  curLevel = curNode.LayerInt;
            Tile.Type curType = curNode.GetLevelType(curLevel);
            if (request.Debug)
            {
                Debug.Log("currenlty checking" + curNode.GridX + " " + curNode.GridY);
            }
            foreach (Node neighbor in grid.GetNeighbors(curNode))
            {




                if (request.Debug)
                {
                    Debug.Log("  --- checking neighbor" + neighbor.GridX + " " + neighbor.GridY);
                }
                Tile.Type neighBhorType = neighbor.GetLevelType(curLevel);
                Tile neighBhor2 = neighbor.GetLevelTile(curLevel);
                int curLevel2 = curLevel;
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }
                if (neighBhorType == Tile.Type.stiars)
                {
                    if (request.Debug)
                    {
                        Debug.Log(" ---- on staits");
                    }

                    if (neighbor.isTransStart(curLevel))
                    {
                        Tile neighbhorTile = neighbor.GetLevelTile(curLevel);
                        if (neighbhorTile != null)
                        {
                            if (request.Debug)
                            {
                                Debug.Log(" >>> found stair start curLevel is " + curLevel + " goin to " + neighbhorTile.startLayerInt);
                            }

                            curLevel = neighbhorTile.startLayerInt;

                        }
                    }
                    else if (neighbor.isTransEnd(curLevel))
                    {
                        Tile neighbhorTile = neighbor.GetLevelTile(curLevel);
                        if (neighbhorTile != null)
                        {
                            if (request.Debug)
                            {
                                Debug.Log(" <<< found stair end curLevel is " + curLevel + " goin to " + neighbhorTile.endLayerInt);
                            }

                            curLevel = neighbhorTile.endLayerInt;
                        }
                    }
                }
                neighBhorType = neighbor.GetLevelType(curLevel);
                // skisp neighbors that are not walkable or in closed
                // which would mean it was found to be not part of the path
                /* if (!(neighbor.LayerInt != curNode.LayerInt && (startNode.type != Tile.Type.stiars && neighbor.type != Tile.Type.building)))
                 {*/
                // Debug.Log(" --- Neighbor " + neighbor.LayerInt + " " + curNode.LayerInt + " was skipped " + neighbor.Walkable + "  " + neighbor.type);
                if ((neighbor.isBlocked(curLevel)) && request.AvoidObstacle)
                {
                    if (request.Debug)
                    {
                        Debug.Log(" was blocked on level " + curLevel);
                    }
                    continue;
                }
                /* }*/
                int newMovementCostToNeighbor = curNode.GCost + GetDistance(curNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    // calculates g cost
                    neighbor.GCost = newMovementCostToNeighbor;
                    // calculates h cost which is the distacne from the target
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = curNode;
                    if (!openSet.Contains(neighbor))
                    {
                        // curLevel = tempLevel;
                        openSet.Add(neighbor);
                        if (request.Debug)
                        {
                            Debug.Log("addibng step " + neighbor.GridX + " " + curNode.GridY + " on level " + curLevel);
                        }
                        // Debug.Log("addibng step " + neighbor.LayerInt + " " + curNode.LayerInt);
                    }
                    else
                    {
                        if (request.Debug)
                        {
                            Debug.Log(">> addibng step " + neighbor.GridX + " " + curNode.GridY + " on level " + curLevel);
                        }
                        // curLevel = tempLevel;

                        openSet.UpdateItem(neighbor);
                    }
                }

                else if (request.Debug)
                {
                    Debug.Log(" was skipped on level " + curLevel);
                }
            }
        }



        // Debug.Log("startNode walk = " + startNodeFeet.Walkable + " target walk = " + targetNode.Walkable);

        // if path has been found
        if (pathSuccess)
        {

            // retraces path to fix order 
            pathAndDir = RetracePath(startNode, targetNode, request.Range);
            if (pathAndDir != null)
            {

                // makes sure path is successfull and records the length of the path
                pathSuccess = pathAndDir.length > 0 && pathAndDir.path.Count > 0;
                pathLength = (int)Vector3.Distance(startNode.WorldPos, targetNode.WorldPos);
                if (pathAndDir.path.Count > request.Range)
                {
                    pathSuccess = false;
                }
            }

            else
            {
                pathSuccess = false;

            }
        }


        // sends the path result back to FinishedProcessingPath
        callback(new PathResult(pathAndDir, pathSuccess, request.Callback, pathLength, startNode, request.Sender, request.Range, targetNode));

    }




    public PathAndDir RetracePath(Node startNode, Node targetNode, int range)
    {
        List<List<Vector3>> pathAndDir = new List<List<Vector3>>();
        List<Node> path = new List<Node>();
        Node curNode = targetNode;
        int count = 0;
        while (curNode != startNode)
        {
            path.Add(curNode);
            curNode = curNode.Parent;
            count++;

        }
        PathAndDir pathAndDir2 = new PathAndDir(new List<Vector3>(), new List<Vector3>(), 0);

        // path.Add(targetNode);
        // path.Reverse();
        //grid.Path = path;

        // makes the waypoints only contain points where the direction changes
        pathAndDir = SimplifyPath(path);
        if (pathAndDir.Count > 1)
        {

            pathAndDir2.path = pathAndDir[0];
            pathAndDir2.dir = pathAndDir[1];
            pathAndDir2.path.Reverse();
            pathAndDir2.dir.Reverse();
            pathAndDir2.length = path.Count;
            return pathAndDir2;
        }
        else
        {
            return null;
        }
    }
    public List<List<Vector3>> SimplifyPath(List<Node> path)
    {
        List<List<Vector3>> pathAndDir = new List<List<Vector3>>();
        List<Vector3> wayPoints = new List<Vector3>();
        List<Vector3> dirs = new List<Vector3>();
        Vector2 dirOld = Vector2.zero;
        if (path.Count > 0)
        {
            wayPoints.Add(path[0].WorldPos);
            dirs.Add(Vector3.zero);
            for (int i = 1; i < path.Count; i++)
            {
                // gets the direction of each path
                Vector2 dirNew = new Vector2(path[i - 1].GridX - path[i].GridX,
                    path[i - 1].GridY - path[i].GridY);
                //Debug.Log(dirNew.x + "  " + dirNew.y);

                // if the direaction changes add the waypoint 
                if (dirOld != dirNew)
                {
                    //wayPoints.Add(path[i].WorldPos);
                    //dirs.Add(new Vector3(dirNew.x, dirNew.y, 0));
                }
                wayPoints.Add(path[i].WorldPos);
                dirs.Add(new Vector3(dirNew.x, dirNew.y, 0));
                dirOld = dirNew;
            }
            if (dirs.Count > 1)
            {
                dirs[0] = dirs[1];
            }
            //dirs[0] = dirs[1];
            pathAndDir.Add(wayPoints);
            pathAndDir.Add(dirs);
        }
        // converts to array
        return pathAndDir;
    }

    public int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
public class PathAndDir
{

    public List<Vector3> path;
    public List<Vector3> dir;
    public int length;
    public PathAndDir()
    {
        path = new List<Vector3>();
        dir = new List<Vector3>();
        length = 0;
    }
    public PathAndDir(List<Vector3> path, List<Vector3> dir, int length)
    {
        this.path = path;
        this.length = length;
        this.dir = dir;
    }
}