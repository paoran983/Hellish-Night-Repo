using UnityEngine;

public class CameraDrag : MonoBehaviour {
    [SerializeField] private Vector3 minBound, maxBound, curUnitPos, boundPos, smoothPos;
    private Vector3 targetPos;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Unit curUnit;
    private float height;
    private float width;
    public bool isMoving;
    [SerializeField] private Vector3 objectToFollow;
    [SerializeField] Vector3 offset;
    [Range(1, 10)][SerializeField] private float smoothFactor;
    [SerializeField] private float combatSmoothFacotr;
    [SerializeField] private float combatMoveSpeed;
    [SerializeField] private Transform defaultObjectToFollow;
    [SerializeField] private Vector3 defaultObjectToFollowPos;
    [SerializeField] private Node nodeToFollow;
    [SerializeField] private float isMovingOffset;
    private bool inCombat;
    private void Start() {
        inCombat = false;
        nodeToFollow = null;
    }
    private void Update() {
        Follow();


    }
    private void Follow() {
        // follows the desired object
        if (!inCombat) {
            defaultObjectToFollowPos = defaultObjectToFollow.position;
            curUnitPos = defaultObjectToFollowPos + offset;
            boundPos = new Vector3(
            Mathf.Clamp(curUnitPos.x, minBound.x, maxBound.x),
            Mathf.Clamp(curUnitPos.y, minBound.y, maxBound.y),
            Mathf.Clamp(curUnitPos.z, minBound.z, maxBound.z));
            smoothPos = Vector3.Lerp(transform.position, curUnitPos, smoothFactor * Time.deltaTime);
            transform.position = smoothPos;

        }
        else if (inCombat) {
            if (nodeToFollow == null) {
                objectToFollow = curUnit.transform.position;
            }
            else {
                objectToFollow = nodeToFollow.WorldPos;
            }
            curUnitPos = objectToFollow + offset;
            boundPos = new Vector3(
            Mathf.Clamp(curUnitPos.x, minBound.x, maxBound.x),
            Mathf.Clamp(curUnitPos.y, minBound.y, maxBound.y),
            Mathf.Clamp(curUnitPos.z, minBound.z, maxBound.z));
            smoothPos = Vector3.Lerp(transform.position, curUnitPos, (smoothFactor * Time.deltaTime));
            transform.position = smoothPos;
        }
        if (Vector3.Distance(transform.position, curUnitPos) > isMovingOffset) {
            isMoving = true;
        }
        else {
            isMoving = false;
        }


    }
    public Unit CurUnit {
        get {
            return curUnit;
        }
        set {
            curUnit = value;
        }
    }

    public bool InCombat {
        get {
            return inCombat;
        }
        set {
            inCombat = value;
        }
    }

    public void ActivateForCombat(Unit firstUnit) {
        inCombat = true;
        AssignUnit(firstUnit);
    }

    public void AssignUnit(Unit unit) {
        if (unit == null) {
            Debug.Log("given null");
            return;
        }

        this.curUnit = unit;
        objectToFollow = curUnit.transform.position;
    }

    public Vector3 ObjectToFollow { get { return objectToFollow; } }
    public Node NodeToFollow { get { return nodeToFollow; } set { nodeToFollow = value; } }
    public Vector3 Offset { get { return offset; } }
}
