using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();
    private static PathRequestManager instance;
    //  private bool isProcessingPath;
    private PathFinding pathFinding;
    private Queue<JobHandle> jobs = new Queue<JobHandle>();
    private void Awake()
    {
        instance = this;
        pathFinding = GetComponent<PathFinding>();
        //  NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);
    }
    private void Update()
    {
        // completes path requests whenever given
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {

                    PathResult result = results.Dequeue();

                    result.Callback(result);
                    // Debug.Log("processing request....");
                }
            }
        }
    }
    public PathFinding PathFinding
    {
        get
        {
            return pathFinding;
        }
    }
    /*---------------------------------------------------------------------
     *  Method RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)
     *
     *  Purpose: Reuests a new path to the postion "pathEnd" from the position "pathStart"
     *  
     *  Parameters: Vector3 pathStart = where the path should starts from
     *              Vector3 pathEnd = where the path should end
     *              Action<Vector3[],bool> callback = Action that will contain the
     *              path is found and a bool that says if the path finding was successful or not
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void RequestPath(PathRequest request)
    {

        ThreadStart threadStart = delegate
        {
            // Debug.Log(" requesting to move");
            instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);

        };
        threadStart.Invoke();
        //  PathFindingJob newJob = new PathFindingJob(request, instance, pathFinding);



    }//RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)

    /*---------------------------------------------------------------------
     *  Method FinishedProcessingPath(Vector3[] path, bool success)
     *
     *  Purpose: Shows that path is done processing and assigns the path if found
     *           and wheater or not the path finding was succesful
     *  
     *  Parameters: Vector3[] path = path if found
     *              bool success = if the path was found or not
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void FinishedProcessingPath(PathResult result)
    {

        lock (results)
        {
            // Debug.Log("path found finnishing request");
            results.Enqueue(result);
        }
    }// FinishedProcessingPath(Vector3[] path, bool success)


}


public struct PathRequest
{
    private Vector3 pathStart;
    private Vector3 pathEnd;
    private Action<PathResult> callback;
    private Unit sender;
    private int range;
    private bool avoidObstacle;
    private bool debug;
    public PathRequest(Unit sender, Vector3 pathStart, Vector3 pathEnd, int range, bool avoidObstacle, Action<PathResult> callback)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
        this.sender = sender;
        this.range = range;
        this.avoidObstacle = avoidObstacle;
        this.debug = false;

    }
    public PathRequest(Unit sender, Vector3 pathStart, Vector3 pathEnd, int range, bool avoidObstacle, bool debug, Action<PathResult> callback)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
        this.sender = sender;
        this.range = range;
        this.avoidObstacle = avoidObstacle;
        this.debug = debug;
    }

    public Unit Sender
    {
        get
        {
            return sender;
        }
        set
        {
            sender = value;
        }
    }

    public int Range
    {
        get
        {
            return range;
        }
        set
        {
            range = value;
        }
    }
    public Vector3 PathStart
    {
        get
        {
            return this.pathStart;
        }
        set
        {
            this.pathStart = value;
        }
    }
    public Vector3 PathEnd
    {
        get
        {
            return this.pathEnd;
        }
        set
        {
            this.pathEnd = value;
        }
    }

    public Action<PathResult> Callback
    {
        get
        {
            return callback;
        }
        set
        {
            callback = value;
        }
    }
    public bool AvoidObstacle { get { return avoidObstacle; } set { avoidObstacle = value; } }
    public bool Debug { get { return debug; } set { debug = value; } }
}
public class PathResult
{
    private PathAndDir pathAndDir;
    private bool success;
    private Action<PathResult> callback;
    private int length;
    private Node startNode;
    private Unit sender;
    private int range;
    private Node endNode;

    public PathResult(PathAndDir pathAndDir, bool success, Action<PathResult> callback, int length, Node startNode, Unit sender, int range, Node endNode)
    {
        this.pathAndDir = pathAndDir;
        this.success = success;
        this.callback = callback;
        this.length = length;
        this.startNode = startNode;
        this.endNode = endNode;
        this.sender = sender;
        this.range = range;
    }

    public bool Success { get { return this.success; } set { this.success = value; } }
    public PathAndDir PathAndDir { get { return this.pathAndDir; } set { this.pathAndDir = value; } }

    public int Length { get { return this.length; } set { this.length = value; } }
    public int Range { get { return range; } set { this.range = value; } }
    public Node StartNode { get { return this.startNode; } set { this.startNode = value; } }
    public Unit Sender { get { return this.sender; } set { this.sender = value; } }
    public Node EndNode { get { return endNode; } set { endNode = value; } }

    public Action<PathResult> Callback
    {
        get
        {
            return callback;
        }
        set
        {
            callback = value;

        }

    }
}
public struct PathFindingJob : IJob
{

    public PathRequest request;
    public PathRequestManager instance;
    public PathFinding pathFinding;

    public PathFindingJob(PathRequest request, PathRequestManager instance, PathFinding pathFinding)
    {
        this.request = request;
        this.pathFinding = pathFinding;
        this.instance = instance;
    }
    public void Execute()
    {
        instance.PathFinding.FindPath(request, instance.FinishedProcessingPath);
    }
}

/*public class PathRequestManager : MonoBehaviour {
    Queue<PathResult> results = new Queue<PathResult>();
    Queue<PathRequest> requests = new Queue<PathRequest>();
    private static PathRequestManager instance;
    //  private bool isProcessingPath;
    private PathFinding pathFinding;
    private NativeQueue<JobHandle> jobs;
    private void Awake() {
        instance = this;
        pathFinding = GetComponent<PathFinding>();
        jobs = new NativeQueue<JobHandle>(Allocator.TempJob);
        //  NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);
    }
    private void Update() {
        // completes path requests whenever given
        if(requests.Count > 0) {
            NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(requests.Count, Allocator.TempJob);
            for (int i = 0; i < requests.Count; i++) {
                PathRequest curRequest = requests.Dequeue();
                pathFinding.FindPath(curRequest, instance.FinishedProcessingPath);
                //int i = requests.Count;

                // PathFindingJob newJob = new PathFindingJob(curRequest, instance, pathFinding);
                //  jobHandleArray[i] = newJob.Schedule();
            }
          //  JobHandle.CompleteAll(jobHandleArray);
           // jobHandleArray.Dispose();
        }
        /*if (results.Count > 0) {
            int itemsInQueue = results.Count;
            lock (results) {
                for (int i = 0; i < itemsInQueue; i++) {

                    PathResult result = results.Dequeue();
                    
                    result.Callback(result.PathAndDir, result.Success, result.Length, result.StartNode, result.Sender);
                    // Debug.Log("processing request....");
                }
                jobs.Dispose();
            }
        }*
    }
    public PathFinding PathFinding {
        get {
            return pathFinding;
        }
    }
    /*---------------------------------------------------------------------
     *  Method RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)
     *
     *  Purpose: Reuests a new path to the postion "pathEnd" from the position "pathStart"
     *  
     *  Parameters: Vector3 pathStart = where the path should starts from
     *              Vector3 pathEnd = where the path should end
     *              Action<Vector3[],bool> callback = Action that will contain the
     *              path is found and a bool that says if the path finding was successful or not
     *
     *  Returns: none
     *-------------------------------------------------------------------*
    public void RequestPath(PathRequest request) {
        Debug.Log(" requesting to move");
        /* ThreadStart threadStart = delegate {
             //  Debug.Log(" requesting to move");
             instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);

         };
         threadStart.Invoke();*
        requests.Enqueue(request);
       // PathFindingJob newJob = new PathFindingJob(request, instance, pathFinding);
      //  jobs.Enqueue(newJob.Schedule());



    }//RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)

    /*---------------------------------------------------------------------
     *  Method FinishedProcessingPath(Vector3[] path, bool success)
     *
     *  Purpose: Shows that path is done processing and assigns the path if found
     *           and wheater or not the path finding was succesful
     *  
     *  Parameters: Vector3[] path = path if found
     *              bool success = if the path was found or not
     *
     *  Returns: none
     *-------------------------------------------------------------------*
    public void FinishedProcessingPath(PathResult result) {

        /* lock (results) {
             // Debug.Log("path found finnishing request");
             results.Enqueue(result);
         }*
        results.Enqueue(result);
       // jobs.Enqueue(result);
    }// FinishedProcessingPath(Vector3[] path, bool success)


}


public struct PathRequest {
    private Vector3 pathStart;
    private Vector3 pathEnd;
    private Action<PathAndDir, bool, int, Node, Unit> callback;
    private Unit sender;

    public PathRequest(Unit sender, Vector3 pathStart, Vector3 pathEnd, Action<PathAndDir, bool, int, Node, Unit> callback) {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
        this.sender = sender;
    }

    public Unit Sender {
        get {
            return sender;
        }
        set {
            sender = value;
        }
    }


    public Vector3 PathStart {
        get {
            return this.pathStart;
        }
        set {
            this.pathStart = value;
        }
    }
    public Vector3 PathEnd {
        get {
            return this.pathEnd;
        }
        set {
            this.pathEnd = value;
        }
    }

    public Action<PathAndDir, bool, int, Node, Unit> Callback {
        get {
            return callback;
        }
        set {
            callback = value;
        }
    }
}
public struct PathResult {
    private PathAndDir pathAndDir;
    private bool success;
    private Action<PathAndDir, bool, int, Node, Unit> callback;
    private int length;
    private Node startNode;
    private Unit sender;

    public PathResult(PathAndDir pathAndDir, bool success, Action<PathAndDir, bool, int, Node, Unit> callback, int length, Node startNode, Unit sender) {
        this.pathAndDir = pathAndDir;
        this.success = success;
        this.callback = callback;
        this.length = length;
        this.startNode = startNode;
        this.sender = sender;
    }

    public bool Success {
        get {
            return this.success;
        }
        set {
            this.success = value;
        }
    }
    public PathAndDir PathAndDir {
        get {
            return this.pathAndDir;
        }
        set {
            this.pathAndDir = value;
        }
    }

    public int Length {
        get {
            return this.length;
        }
        set {
            this.length = value;
        }
    }
    public Node StartNode {
        get {
            return this.startNode;
        }
        set {
            this.startNode = value;
        }
    }
    public Unit Sender {
        get {
            return this.sender;
        }
        set {
            this.sender = value;
        }
    }

    public Action<PathAndDir, bool, int, Node, Unit> Callback {
        get {
            return callback;
        }
        set {
            callback = value;

        }

    }
}
public struct PathFindingJob : IJob {

    public PathRequest request;
    public PathRequestManager instance;
    public PathFinding pathFinding;

    public PathFindingJob(PathRequest request, PathRequestManager instance, PathFinding pathFinding) {
        this.request = request;
        this.pathFinding = pathFinding;
        this.instance = instance;
    }
    public void Execute() {
          pathFinding.FindPath(request, instance.FinishedProcessingPath);
    }
}

public class PathRequestManager : MonoBehaviour {
    Queue<PathResult> results = new Queue<PathResult>();
    private static PathRequestManager instance;
    //  private bool isProcessingPath;
    private PathFinding pathFinding;
    private Queue<JobHandle> jobs = new Queue<JobHandle>();
    private float startTime = 0;
    private void Awake() {
        instance = this;
        pathFinding = GetComponent<PathFinding>();
      //  NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);
    }
    private void Update() {
        
        // completes path requests whenever given
        if (jobs.Count > 0) {
            int itemsInQueue = jobs.Count;
         //   lock (results) {
                for (int i = 0; i < itemsInQueue; i++) {
                    
                    JobHandle curJubHandle = jobs.Dequeue();
                    curJubHandle.Complete();
                // run the function of the result
                     
                   // result.Callback(result.PathAndDir, result.Success, result.Length,result.StartNode,result.Sender);
                    Debug.Log((((Time.realtimeSinceStartup - startTime))*1000f)+"ms");
                   // Debug.Log("processing request....");
                }
           // }
        }
    }
    public PathFinding PathFinding {
        get {
            return pathFinding;
        }
    }
    /*---------------------------------------------------------------------
     *  Method RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)
     *
     *  Purpose: Reuests a new path to the postion "pathEnd" from the position "pathStart"
     *  
     *  Parameters: Vector3 pathStart = where the path should starts from
     *              Vector3 pathEnd = where the path should end
     *              Action<Vector3[],bool> callback = Action that will contain the
     *              path is found and a bool that says if the path finding was successful or not
     *
     *  Returns: none
     *-------------------------------------------------------------------
    public void RequestPath(PathRequest request) {

        /*  ThreadStart threadStart = delegate {
            //  Debug.Log(" requesting to move");
              instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);

          };
          threadStart.Invoke();
         startTime = Time.realtimeSinceStartup;
        //  instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);

        //  PathFindingJob newJob = new PathFindingJob(request, instance, pathFinding);
        Debug.Log("Requesting path job...");
        PathFindingJob job = new PathFindingJob(request);
        jobs.Enqueue(job.Schedule());




    }
  
    //RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)

    /*---------------------------------------------------------------------
     *  Method FinishedProcessingPath(Vector3[] path, bool success)
     *
     *  Purpose: Shows that path is done processing and assigns the path if found
     *           and wheater or not the path finding was succesful
     *  
     *  Parameters: Vector3[] path = path if found
     *              bool success = if the path was found or not
     *
     *  Returns: none
     *-------------------------------------------------------------------
    public void FinishedProcessingPath(PathResult result) {

        /*lock (results) {
           // Debug.Log("path found finnishing request");
            results.Enqueue(result);
        }
        results.Enqueue(result);
    }// FinishedProcessingPath(Vector3[] path, bool success)


}



    public struct PathRequest {
    private Vector3 pathStart;
    private Vector3 pathEnd;
    private Action<PathAndDir, bool,int,Node,Unit> callback;
    private Unit sender;

    public PathRequest(Unit sender,Vector3 pathStart, Vector3 pathEnd, Action<PathAndDir, bool, int,Node,Unit> callback) {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
        this.sender = sender;
    }

    public Unit Sender {
        get {
            return sender;
        }
        set { 
            sender = value; 
        }
    }


    public Vector3 PathStart {
        get {
             return this.pathStart;
        }
        set {
            this.pathStart = value;
        }
    }
    public Vector3 PathEnd {
        get {
            return this.pathEnd;
        }
        set {
            this.pathEnd = value;
        }
    }

    public Action<PathAndDir, bool, int,Node,Unit> Callback {
        get {
            return callback;
        }
        set {
            callback = value;
        }
    }
}
public struct PathResult {
    private PathAndDir pathAndDir;
    private bool success;
    private Action<PathAndDir, bool, int,Node,Unit> callback;
    private int length;
    private Node startNode;
    private Unit sender;

    public PathResult(PathAndDir pathAndDir, bool success, Action<PathAndDir, bool, int,Node,Unit> callback, int length, Node startNode,Unit sender) {
        this.pathAndDir = pathAndDir;
        this.success = success;
        this.callback = callback;
        this.length = length;
        this.startNode = startNode;
        this.sender = sender;
    }
    
    public bool Success {
        get {
            return this.success;
        }
        set {
            this.success = value;
        }
    }
    public PathAndDir PathAndDir {
        get {
            return this.pathAndDir;
        }
        set {
            this.pathAndDir = value;
        }
    }

    public int Length {
        get { 
            return this.length; 
        }
        set {
            this.length = value;
        }
    }
    public Node StartNode {
        get {
            return this.startNode;
        }
        set {
            this.startNode = value;
        }
    }
    public Unit Sender {
        get {
            return this.sender;
        }
        set {
            this.sender = value;
        }
    }

    public Action<PathAndDir, bool, int,Node,Unit> Callback {
        get {
            return callback;
        }
        set {
            callback = value;

        }

    }
}
public struct PathFindingJob : IJob {

    public PathRequest request;
  //  public PathRequestManager instance;
   // public PathFinding pathFinding;
    //private float startTime;

    public PathFindingJob(PathRequest request) {
        this.request = request;
      //  this.pathFinding = pathFinding;
       // this.instance = instance;
      //  startTime = 0f;
    }
    public void Execute() {
        //startTime = Time.realtimeSinceStartup;
        //request.Sender.PathFinding.FindPath(request, request.Sender.PathRequestManager.FinishedProcessingPath);
       // instance.PathFinding.FindPath(request,instance.FinishedProcessingPath);
    }

    /*---------------------------------------------------------------------
    *  Method RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)
    *
    *  Purpose: Reuests a new path to the postion "pathEnd" from the position "pathStart"
    *  
    *  Parameters: Vector3 pathStart = where the path should starts from
    *              Vector3 pathEnd = where the path should end
    *              Action<Vector3[],bool> callback = Action that will contain the
    *              path is found and a bool that says if the path finding was successful or not
    *
    *  Returns: none
    *-------------------------------------------------------------------
    public void RequestPath(PathRequest request) {

        /*  ThreadStart threadStart = delegate {
            //  Debug.Log(" requesting to move");
              instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);

          };
          threadStart.Invoke();*/
//    startTime = Time.realtimeSinceStartup;
// pathFinding.FindPath(request, instance.FinishedProcessingPath);
//  PathFindingJob newJob = new PathFindingJob(request, instance, pathFinding);



//RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)

/*---------------------------------------------------------------------
 *  Method FinishedProcessingPath(Vector3[] path, bool success)
 *
 *  Purpose: Shows that path is done processing and assigns the path if found
 *           and wheater or not the path finding was succesful
 *  
 *  Parameters: Vector3[] path = path if found
 *              bool success = if the path was found or not
 *
 *  Returns: none
 *-------------------------------------------------------------------
public void FinishedProcessingPath(PathResult result) {

    /*lock (results) {
       // Debug.Log("path found finnishing request");
        results.Enqueue(result);
    }
    //result.Callback(result.PathAndDir, result.Success, result.Length, result.StartNode, result.Sender);
  //  Debug.Log((((Time.realtimeSinceStartup - startTime)) * 1000f) + "ms");
    // results.Enqueue(result);
}// FinishedProcessingPath(Vector3[] path, bool success)
}
*/

