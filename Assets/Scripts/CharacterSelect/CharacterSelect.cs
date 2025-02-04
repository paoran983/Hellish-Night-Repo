using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;

    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private string initalScene;
    [SerializeField] private List<Unit> units;
    [SerializeField] private State state;
    private List<Unit> blueTeamList, redTeamList;
    private int curTurn;
    private enum State
    {
        Normal,
        Waiting
    }
    private void Start()
    {
        DisableButtonsDependingOnData();

    }
    public void RunCombatEncounter()
    {
        switch (state)
        {
            // if player is not in combar or waiting for command
            case State.Normal:



                break;
            // if player is moving or in the middle of an action
            case State.Waiting:
                break;
        }
        return;
    }

    /*---------------------------------------------------------------------
     *  Method: DisableButtonsDependingOnData()
     *
     *  Purpose: Shows valid buttosn in main menu
     *             
     *  Parameters: none
     * Returns: none
     *-------------------------------------------------------------------*/
    private void DisableButtonsDependingOnData()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            continueGameButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }

    public void OnNewGameClicked()
    {

        saveSlotsMenu.ActivateMenu(false);
        this.DeactivateMenu();
    }

    public void OnLoadGameClicked()
    {
        saveSlotsMenu.ActivateMenu(true);
        this.DeactivateMenu();
    }

    public void OnContinueGameClicked()
    {
        Debug.Log("continue");
        DisableMenuButtons();
        // save the game anytime before loading a new scene
        DataPersistenceManager.instance.SaveGame();
        // load the next scene - which will in turn load the game because of 
        // OnSceneLoaded() in the DataPersistenceManager
        SceneManager.LoadSceneAsync(initalScene);
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continueGameButton.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
        DisableButtonsDependingOnData();
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
