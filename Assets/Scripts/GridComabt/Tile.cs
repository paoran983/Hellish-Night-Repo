using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile : MonoBehaviour
{
    [SerializeField] public Collider2D start;
    [SerializeField] public Collider2D end;
    [SerializeField] public Collider2D stairRails;
    [SerializeField] public string startLayer;
    [SerializeField] public string endLayer;
    [SerializeField] public int startLayerInt;
    [SerializeField] public int endLayerInt;
    [SerializeField] public Type type;
    [SerializeField] public bool isWet, isConductive, isFire, isFlamable, isCharged, isDamaged, isBroken;
    [SerializeField] private SerializableDictionary<int, Type> typeLevels;


    public enum Type
    {
        deafult,
        floor,
        building,
        prop,
        stiars,
        bridge,
        ledge,
        ladder,
        hole,
        water,
        stairStart,
        stairEnd,
        stairRail
    }

    public void EnterStairs(Collider2D collision, Unit unit)
    {
        String sortingLayer = unit.gameObject.GetComponent<SpriteRenderer>().sortingLayerName;
        int layerDif = Math.Abs(endLayerInt - startLayerInt);
        if (collision == this.start)

        {



            if (sortingLayer.Equals(this.startLayer) == false)
            {
                //  Debug.Log(sortingLayer + " start " + this.startLayer + "  " + sortingLayer.Equals(this.startLayer) + "  " + layerDif);
                unit.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = this.startLayer;
                unit.gameObject.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<TilemapRenderer>().sortingOrder + 1;
                unit.CurLayer = startLayerInt;
                unit.ChangeHeight(layerDif);
                unit.IgnoreCollisionLayer(startLayerInt, endLayerInt);
            }

        }
        else if (collision == this.end)

        {


            if (sortingLayer.Equals(this.endLayer) == false)
            {
                // Debug.Log(" end " + this.endLayer);
                unit.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = this.endLayer;
                unit.gameObject.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<TilemapRenderer>().sortingOrder + 1;
                unit.ChangeHeight((int)(layerDif * -1));
                unit.CurLayer = endLayerInt;
                unit.IgnoreCollisionLayer(endLayerInt, startLayerInt);
            }
        }
    }
    public void EnterBridge(Collider2D collision, Unit unit)
    {
        String sortingLayer = this.gameObject.GetComponent<SpriteRenderer>().sortingLayerName;
        Debug.Log("_-entering under bridge " + collision.gameObject.name);
        foreach (Collider2D child in collision.GetComponentsInChildren<Collider2D>())
        {
            if (child.GetComponent<TilemapRenderer>() != null)
            {
                Debug.Log("child is on " + child.GetComponent<TilemapRenderer>().sortingLayerName);
                if (child.GetComponent<TilemapRenderer>().sortingLayerName != sortingLayer)
                {
                    if (child.GetComponent<Tile>() != null)
                    {

                        if (child.GetComponent<Tile>().type == Tile.Type.floor)
                        {
                            continue;
                        }

                    }
                    Debug.Log("child is on " + child.GetComponent<TilemapRenderer>().sortingLayerName);
                    child.GetComponent<Collider2D>().isTrigger = true;
                }
            }

        }
    }
    public void LeaveBridge(Collider2D collision, Unit unit)
    {
        String sortingLayer = this.gameObject.GetComponent<SpriteRenderer>().sortingLayerName;
        Debug.Log("_-entering under bridge " + collision.gameObject.name);
        foreach (Collider2D child in collision.GetComponentsInChildren<Collider2D>())
        {
            if (child.GetComponent<TilemapRenderer>() != null)
            {
                Debug.Log("child is on " + child.GetComponent<TilemapRenderer>().sortingLayerName);
                if (child.GetComponent<TilemapRenderer>().sortingLayerName != sortingLayer)
                {
                    if (child.GetComponent<Tile>() != null)
                    {

                        if (child.GetComponent<Tile>().type == Tile.Type.floor)
                        {
                            continue;
                        }

                    }
                    Debug.Log("child is on " + child.GetComponent<TilemapRenderer>().sortingLayerName);
                    child.GetComponent<Collider2D>().isTrigger = true;
                }
            }

        }
    }

}
