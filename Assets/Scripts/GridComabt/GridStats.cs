
using UnityEngine;
/*---------------------------------------------------------------------
 *  Class: GridStats: MonoBehavior 
 *
 *  Purpose: Keep track of a cell in a grid and its postion and if its been
 *           visited or not.
 *
 *  Fields: private int visited = if the cell has been visited 1 if not and 0 is yes
 *               private int x,y = the current x and y coordinates of the cell
 *
 *-------------------------------------------------------------------*/
public class GridStats : MonoBehaviour {
    [SerializeField] private int visited = 1;
    [SerializeField] private int x = 0;
    [SerializeField] private int y = 0;
    [SerializeField] private bool valid = true;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    public int GetVisited() {
        return visited;
    }
    public int GetX() {
        return x;
    }
    public int GetY() {
        return y;
    }
    public bool GetValid() { return valid; }
    public void SetValid(bool valid) {  this.valid = valid; }
    public void SetX(int x) { this.x = x; }
    public void SetY(int y) { this.y = y; }

    public void SetVisited(int vis) { this.visited = vis; }

    public void Setup(int x,int y,int visited) {
        this.x = x;
        this.y = y;
        this.visited = visited;

    }
    public void CheckValid() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up * -1,1);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, transform.right * -1,1);

        Debug.DrawRay(transform.position, transform.right * -1, Color.LerpUnclamped(Color.red, Color.red, x));
        Debug.DrawRay(transform.position, transform.up * -1, Color.LerpUnclamped(Color.blue, Color.blue, x));
        if (hit.collider != null && hit2.collider != null) {

            // targetPos = hit.collider.gameObject.transform.position;
            //Debug.Log(">>  (" + x + "," + y + ")  " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.layer != 9 && hit2.collider.gameObject.layer != 9) {
                if (hit.collider.gameObject.GetInstanceID() == hit2.collider.gameObject.GetInstanceID()) {
                //    Debug.Log(">>  (" + x + "," + y + ")  " + hit.collider.gameObject.name+" dis = "+hit.transform.localScale+" , "+hit2.transform.localScale);
                    valid = false;
                  // gameObject.SetActive(false);
                    
                }
            }
            

        }
        else {
            valid = true;
        }

        gameObject.SetActive(valid);

    }

}