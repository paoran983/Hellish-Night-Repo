using Inventory.UI;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private InventoryItemUI item;
    [SerializeField] private InventoryItemDescriptionUI description;
    [SerializeField] private TurnCountUIController turnCounter;
    private int offset;

    public void Awake()
    {

        //canvas =GetComponent<Canvas>();
        description = GetComponentInChildren<InventoryItemDescriptionUI>();
        item = GetComponentInChildren<InventoryItemUI>();
        Toggle(false);
        if (description != null)
        {
            ToggleDescription(false);
        }
        turnCounter.CreateTurnOrdes(4);
        turnCounter.InitalizeTurns(0);
        turnCounter.Activate(false);

    }
    public void SetData(Sprite sprite, int count)
    {
        item.SetData(sprite, count);
    }
    public void SetDescriptionData(Sprite sprite, string itemName, string itemDescription)
    {
        description.SetDescription(sprite, itemName, itemDescription);
    }
    public void Clear()
    {
        item.SetData(null, 0);
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    /*---------------------------------------------------------------------
 |  Method Show()
 |
 |  Purpose: Follows object with mouse 
 |  
 |   Parameters: Sprite sprite = the sprite to fill current slot
 |               string itemName = name of item in current slot
 |               string itemDescription = description of item in current slot
 |
 |  Returns: none
 *-------------------------------------------------------------------*/
    public void Follow()
    {
        // getting postion of mouse
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform,
            Input.mousePosition, canvas.worldCamera, out position);
        transform.position = canvas.transform.TransformPoint(new Vector3(position.x + offset, position.y));
    }

    public void Toggle(bool val)
    {
        //Debug.Log($"Item Toggle {val}");
        offset = 0;
        if (description != null)
        {
            description.gameObject.SetActive(false);
        }
        if (item != null)
        {
            item.gameObject.SetActive(val);
        }

    }
    public void ToggleDescription(bool val)
    {
        if (val)
        {
            offset = 120;
        }
        else
        {
            offset = 0;
        }

        item.gameObject.SetActive(false);
        description.gameObject.SetActive(val);

    }
    public void TurnCounter(bool val)
    {
        if (val)
        {
            offset = 120;
        }
        else
        {
            offset = 0;
        }

        item.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
        turnCounter.Activate(val);

    }
    public void SetTurnCounter(int val)
    {
        turnCounter.SetTurnCount(val);
    }
    public void AdvanceTurnCounter(int movesMade)
    {
        turnCounter.CompleteTurn(movesMade);
    }
    public void InitliazeTurnCounter(int size)
    {
        turnCounter.InitalizeTurns(size);
    }
    public void ResetTurnCounter()
    {
        turnCounter.ResetTurns();
    }

    void Update()
    {
        Follow();

    }
    public InventoryItemDescriptionUI Description
    {
        get
        {
            return description;
        }
    }
}
