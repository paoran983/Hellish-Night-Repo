using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour {
    [Header("Profile")]
    [SerializeField] private string profileId = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;
    [SerializeField] private TextMeshProUGUI percentageCompleteText;
    [SerializeField] private TextMeshProUGUI deathCountText;

    [Header("Clear Data Button")]
    [SerializeField] private Button clearButton;

    public bool hasData { get; private set; } = false;

    [SerializeField] private Button saveSlotButton;

    private void Awake() {
        saveSlotButton = this.GetComponent<Button>();
    }

    /*---------------------------------------------------------------------
     *  Method: SetData(GameData data) 
     *
     *  Purpose: Takes in game data ojects and sets slot as 
     *           active and fills it with vaid data
     *             
     *  Parameters: GameData data = data to display 
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetData(GameData data) {
        // there's no data for this profileId
        if (data == null) {
            hasData = false;
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            clearButton.gameObject.SetActive(false);
        }
        // there is data for this profileId
        else {
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton.gameObject.SetActive(true);

            percentageCompleteText.text = profileId;
            deathCountText.text = "Level: " + data.curPlayerLevel;
        }
    }

    public string GetProfileId() {
        return this.profileId;
    }

    public void SetInteractable(bool interactable) {
        saveSlotButton.interactable = interactable;
        clearButton.interactable = interactable;
    }
}