using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu {
    [Header("Menu Navigation")]
    [SerializeField] private MainMenu mainMenu;

    [Header("Menu Buttons")]
    [SerializeField] private Button backButton;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;

    [SerializeField] private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    private void Awake() {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }


    /*---------------------------------------------------------------------
     *  Method: OnSaveSlotClicked(SaveSlot saveSlot)
     *
     *  Purpose: Loads a save when a save slot is clicked if from load gme, or 
     *           makes a new game if clicked on empty slot, or brings up 
     *           popup menu if trying ot make new game on existing slot and 
     *           shows that slot as selected
     *
     * Parameters: SaveSlot saveSlot = saveslot that was clicked
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void OnSaveSlotClicked(SaveSlot saveSlot) {
        // disable all buttons
        DisableMenuButtons();
        // case - loading game
        if (isLoadingGame) {
            Debug.Log("loading");
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            SaveGameAndLoadScene();
        }
        // case - new game, but the save slot has data
        else if (saveSlot.hasData) {
            confirmationPopupMenu.ActivateMenu(
                "Starting a New Game with this slot will override the currently saved data. Are you sure?",
                // function to execute if we select 'yes'
                () => {
                    DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
                    DataPersistenceManager.instance.NewGame();
                    SaveGameAndLoadScene();
                },
                // function to execute if we select 'cancel'
                () => {
                    this.ActivateMenu(isLoadingGame);
                }
            );
        }
        // case - new game, and the save slot has no data
        else {
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            DataPersistenceManager.instance.NewGame();
            SaveGameAndLoadScene();
        }
    }

    /*---------------------------------------------------------------------
     *  Method: SaveGameAndLoadScene(SaveSlot saveSlot)
     *
     *  Purpose: Saves the game and loads the game scene
     *
     * Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    private void SaveGameAndLoadScene() {
        // save the game anytime before loading a new scene
        DataPersistenceManager.instance.SaveGame();
        // load the scene
        SceneManager.LoadSceneAsync("SampleScene1");
    }

    /*---------------------------------------------------------------------
     *  Method: OnClearClicked(SaveSlot saveSlot)
     *
     *  Purpose: Disables other buttons and shows popup menu
     *           if yes the save slot is cleared and if not the 
     *           slot is loaded
     *
     * Parameters: SaveSlot saveSlot = slot to be cleared
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void OnClearClicked(SaveSlot saveSlot) {
        DisableMenuButtons();

        confirmationPopupMenu.ActivateMenu(
            "Are you sure you want to delete this saved data?",
            // function to execute if we select 'yes'
            () => {
                DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileId());
                ActivateMenu(isLoadingGame);
            },
            // function to execute if we select 'cancel'
            () => {
                ActivateMenu(isLoadingGame);
            }
        );
    }

    public void OnBackClicked() {
        mainMenu.ActivateMenu();
        this.DeactivateMenu();
    }

    /*---------------------------------------------------------------------
     *  Method: ActivateMenu(bool isLoadingGame)
     *
     *  Purpose: Show the save slots and fill then with data
     *
     * Parameters: bool isLoadingGame = to show or hide
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ActivateMenu(bool isLoadingGame) {
        // set this menu to be active
        this.gameObject.SetActive(true);

        // set mode
        this.isLoadingGame = isLoadingGame;

        // load all of the profiles that exist
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        // ensure the back button is enabled when we activate the menu
        backButton.interactable = true;

        // loop through each save slot in the UI and set the content appropriately
        GameObject firstSelected = backButton.gameObject;
        foreach (SaveSlot saveSlot in saveSlots) {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame) {

                saveSlot.SetInteractable(false);

            }
            else {
                saveSlot.SetInteractable(true);
                if (firstSelected.Equals(backButton.gameObject)) {
                    firstSelected = saveSlot.gameObject;
                }
            }
        }

        // set the first selected button
        Button firstSelectedButton = firstSelected.GetComponent<Button>();
        this.SetFirstSelected(firstSelectedButton);
    }

    public void DeactivateMenu() {
        this.gameObject.SetActive(false);
    }

    private void DisableMenuButtons() {
        foreach (SaveSlot saveSlot in saveSlots) {
            saveSlot.SetInteractable(false);
        }
        backButton.interactable = false;
    }
}