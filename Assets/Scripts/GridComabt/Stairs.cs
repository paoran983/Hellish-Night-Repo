using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] public Collider2D top;
    [SerializeField] public Collider2D bottom;
    [SerializeField] public SortingLayer topLayer;
    [SerializeField] public string bottomLayer;
    [SerializeField] public GameObject bottomWalls;
    [SerializeField] public GameObject topWalls;
    [SerializeField] public GameObject stairWalls;


}
