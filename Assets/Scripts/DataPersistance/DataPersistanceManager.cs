using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour {
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private bool autoSave = false;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    [Header("Auto Saving Configuration")]
    [SerializeField] private float autoSaveTimeSeconds = 60f;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    [SerializeField] private List<Unit> unitList;
    private string selectedProfileId = "";

    private Coroutine autoSaveCoroutine;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake() {
        // makes sure there is only one instnace of manager in scene
        if (instance != null) {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (disableDataPersistence) {
            Debug.LogWarning("Data Persistence is currently disabled!");
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

        InitializeSelectedProfileId();
    }

    private void OnApplicationQuit() {
        SaveGame();
    }
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /*---------------------------------------------------------------------
     *  Method: OnSceneLoaded(Scene scene, LoadSceneMode mode)
     *
     *  Purpose: Set up scene for data persistece and load game save
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        this.unitList = FindAllUnits();
        LoadGame();
        if (autoSave) {
            // start up the auto saving coroutine
            if (autoSaveCoroutine != null) {
                StopCoroutine(autoSaveCoroutine);
            }
            autoSaveCoroutine = StartCoroutine(AutoSave());
        }

    }

    /*---------------------------------------------------------------------
     *  Method: ChangeSelectedProfileId(string newProfileId)
     *
     *  Purpose: Sets the current profileId and loads taht profile
     *             
     *  Parameters: string newProfileId = profile to load
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ChangeSelectedProfileId(string newProfileId) {
        // update the profile to use for saving and loading
        this.selectedProfileId = newProfileId;
        // load the game, which will use that profile, updating our game data accordingly
        LoadGame();
    }

    /*---------------------------------------------------------------------
     *  Method: DeleteProfileData(string newProfileId)
     *
     *  Purpose: Deletes a profiles savefile as 
     *           well as reset the profileId
     *             
     *  Parameters: string newProfileId = profile to delete
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void DeleteProfileData(string profileId) {
        // delete the data for this profile id
        dataHandler.Delete(profileId);
        // initialize the selected profile id
        InitializeSelectedProfileId();
        // reload the game so that our data matches the newly selected profile id
        LoadGame();
    }

    private void InitializeSelectedProfileId() {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();
        if (overrideSelectedProfileId) {
            this.selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode selected profile id with test id: " + testSelectedProfileId);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: NewGame()
     *
     *  Purpose: initlaize gameData to be new game data object
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void NewGame() {
        this.gameData = new GameData();
    }

    /*---------------------------------------------------------------------
     *  Method: LoadGame()
     *
     *  Purpose: Load data from file data handler or initlaize game data
     *           as new game if nothing in file data handler and then push 
     *           data to all scripts that need it
     *             
     *  Parameters: none
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void LoadGame() {
        // return right away if data persistence is disabled
        if (disableDataPersistence) {
            return;
        }
        if (dataHandler == null) {
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        }
        // load any saved data from a file using the data handler
        this.gameData = dataHandler.Load(selectedProfileId);

        // start a new game if the data is null and we're configured to initialize data for debugging purposes
        if (this.gameData == null && initializeDataIfNull) {
            NewGame();
        }

        // if no data can be loaded, don't continue
        if (this.gameData == null) {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }
        if (dataPersistenceObjects == null) {
            dataPersistenceObjects = FindAllDataPersistenceObjects();
        }
        // push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    /*---------------------------------------------------------------------
     *  Method: SaveGame()
     *
     *  Purpose: pass data to other scripts to be updated
     *           then save data to file using file data handler
     *             
     *  Parameters: none
     * Returns: none
     *-------------------------------------------------------------------*/
    public void SaveGame() {
        // return right away if data persistence is disabled
        if (disableDataPersistence) {
            return;
        }

        // if we don't have any data to save, log a warning here
        if (this.gameData == null && !initializeDataIfNull) {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
            return;
        }
        if (dataPersistenceObjects == null) {
            dataPersistenceObjects = FindAllDataPersistenceObjects();
        }
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.SaveData(gameData);
        }

        // timestamp the data so we know when it was last saved
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        // save that data to a file using the data handler
        dataHandler.Save(gameData, selectedProfileId);
    }



    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        // finds all IDataPersistence objects
        // FindObjectsofType takes in an optional boolean to include inactive gameobjects
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();
        List<IDataPersistence> data = new List<IDataPersistence>(dataPersistenceObjects);

        return data;
    }
    private List<Unit> FindAllUnits() {
        // finds all unit objects
        // FindObjectsofType takes in an optional boolean to include inactive gameobjects
        IEnumerable<Unit> units = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<Unit>();
        List<Unit> data = new List<Unit>(units);

        return data;
    }

    public bool HasGameData() {
        return gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData() {
        return dataHandler.LoadAllProfiles();
    }

    /*---------------------------------------------------------------------
     *  Method: AutoSave()
     *
     *  Purpose: Saves the game every few seconds
     *             
     *  Parameters: none
     * Returns: none
     *-------------------------------------------------------------------*/
    private IEnumerator AutoSave() {
        while (true) {
            yield return new WaitForSeconds(autoSaveTimeSeconds);
            SaveGame();
            Debug.Log("Auto Saved Game");
        }
    }
}