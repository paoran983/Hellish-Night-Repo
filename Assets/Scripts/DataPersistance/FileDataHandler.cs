using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler {
    // directory path to save data
    private string dataDirPath = "";
    // name of file to save data to
    private string dataFileName = "";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "password";
    private readonly string backupExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    /*---------------------------------------------------------------------
    *  Method: Save(GameData data, string profileId) 
    *
    *  Purpose: Takes in gameData and a profileID to write and save the gameData to a Json file. 
    *           It does this by first creating a filePath using the given profileID. Then 
    *           creates a directory for the file to be written to. Then it serializes the C# data to Json.
    *           Then it encrpyts the data if needed. Then it writes the serialized data to the file.
    *           and finally tests the file and backs up the data if valid. If not it attempots 
    *           to roll the save back to the last valid backup
    *             
    *  Parameters: GameData data = data to load
    *              string profileId = profile of save file
    *  Returns: none
    *-------------------------------------------------------------------*/
    public GameData Load(string profileId, bool allowRestoreFromBackup = true) {

        // base case - if the profileId is null, return right away
        if (profileId == null) {
            return null;
        }

        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath)) {
            try {
                // load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // optionally decrypt the data
                if (useEncryption) {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            // if errors while reading file it attempts to roll back to last valid save
            catch (Exception e) {
                // since we're calling Load(..) recursively, we need to account for the case where
                // the rollback succeeds, but data is still failing to load for some other reason,
                // which without this check may cause an infinite recursion loop.
                if (allowRestoreFromBackup) {
                    Debug.LogWarning("Failed to load data file. Attempting to roll back.\n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess) {
                        // try to load again recursively
                        loadedData = Load(profileId, false);
                    }
                }
                // if we hit this else block, one possibility is that the backup file is also corrupt
                else {
                    Debug.LogError("Error occured when trying to load file at path: "
                        + fullPath + " and backup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    /*---------------------------------------------------------------------
    *  Method: Save(GameData data, string profileId) 
    *
    *  Purpose: Takes in gameData and a profileID to write and save the gameData to a Json file. 
    *           It does this by first creating a filePath using the given profileID. Then 
    *           creates a directory for the file to be written to. Then it serializes the C# data to Json.
    *           Then it encrpyts the data if needed. Then it writes the serialized data to the file.
    *           and finally tests the file and backs up the data if valid, AS well as back it up 
    *           by making a copt of the file.
    *             
    *  Parameters: GameData data = data to save
    *              string profileId = profile of save file
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void Save(GameData data, string profileId) {
        // base case - if the profileId is null, return right away
        if (profileId == null) {
            return;
        }

        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        string backupFilePath = fullPath + backupExtension;
        // catch any errors while writting file
        try {
            // create the directory or folder the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            // optionally encrypt the data
            if (useEncryption) {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized data to the file
            // using blocks ensure connecntion to file is closed when done reading/writting 
            // creates fileSream to create to new file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) {
                // makes StreamWriter to write to file made by stream
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            GameData verifiedGameData = Load(profileId);
            // if the data can be verified, back it up
            if (verifiedGameData != null) {
                File.Copy(fullPath, backupFilePath, true);
            }
            // otherwise, something went wrong and we should throw an exception
            else {
                throw new Exception("Save file could not be verified and backup could not be created.");
            }

        }
        catch (Exception e) {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
    /*---------------------------------------------------------------------
    *  Method: Delete(string profileId) 
    *
    *  Purpose: Takes in a profileId as a stirng and finds a 
    *           savefile assocaited and deletes it if found
    *             
    *  Parameters: string profileId = profile of file to delete
    *           
    *  Returns: none
    *-------------------------------------------------------------------*/
    public void Delete(string profileId) {
        // base case - if the profileId is null, return right away
        if (profileId == null) {
            return;
        }

        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try {
            // ensure the data file exists at this path before deleting the directory
            if (File.Exists(fullPath)) {
                // delete the profile folder and everything within it
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else {
                Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed to delete profile data for profileId: "
                + profileId + " at path: " + fullPath + "\n" + e);
        }
    }

    /*---------------------------------------------------------------------
    *  Method: LoadAllProfiles() 
    *
    *  Purpose: Creates a dictonary containing the gamedata for each valid profile
    *             
    *  Parameters: none
    *  Returns: profileDictionary = the gamedata for each valid profile
    *-------------------------------------------------------------------*/
    public Dictionary<string, GameData> LoadAllProfiles() {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();

        // loop over all directory names in the data directory path
        // gives collections of direcotry infos
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos) {
            string profileId = dirInfo.Name;

            // defensive programming - check if the data file exists
            // if it doesn't, then this folder isn't a profile and should be skipped
            string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if (!File.Exists(fullPath)) {
                Debug.LogWarning("Skipping directory when loading all profiles because it does not contain data: "
                    + profileId);
                continue;
            }

            // load the game data for this profile and put it in the dictionary
            GameData profileData = Load(profileId);
            // defensive programming - ensure the profile data isn't null,
            // because if it is then something went wrong and we should let ourselves know
            if (profileData != null) {
                profileDictionary.Add(profileId, profileData);
            }
            else {
                Debug.LogError("Tried to load profile but something went wrong. ProfileId: " + profileId);
            }
        }

        return profileDictionary;
    }

    /*---------------------------------------------------------------------
    *  Method: GetMostRecentlyUpdatedProfileId
    *
    *  Purpose: Gets the most recent save profileId
    *             
    *  Parameters: none
    *  Returns: none
    *-------------------------------------------------------------------*/
    public string GetMostRecentlyUpdatedProfileId() {
        string mostRecentProfileId = null;

        Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
        foreach (KeyValuePair<string, GameData> pair in profilesGameData) {
            string profileId = pair.Key;
            GameData gameData = pair.Value;

            // skip this entry if the gamedata is null
            if (gameData == null) {
                continue;
            }

            // if this is the first data we've come across that exists, it's the most recent so far
            if (mostRecentProfileId == null) {
                mostRecentProfileId = profileId;
            }
            // otherwise, compare to see which date is the most recent
            else {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                // the greatest DateTime value is the most recent
                if (newDateTime > mostRecentDateTime) {
                    mostRecentProfileId = profileId;
                }
            }
        }
        return mostRecentProfileId;
    }
    /*---------------------------------------------------------------------
    *  Method: EncryptDecrypt(string data)
    *
    *  Purpose: Encrypts dats using the XOR encrytion by goiunf through each char and 
    *           do a XOR operation the original data and index in the encrytion code word  
    *  Parameters: String data = data to encrypt
    *  Returns: string modifiedData = encryptedData
    *-------------------------------------------------------------------*/
    private string EncryptDecrypt(string data) {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++) {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }

    /*---------------------------------------------------------------------
    *  Method: AttemptRollback(string fullPath)
    *
    *  Purpose: Check if backup file of path proveded exists and if it does 
    *           it copies from the backup file and if not gives a warning
    *             
    *  Parameters: string fullPath = path to file to backup
    *  Returns: bool success = if the rollback was successful or not
    *-------------------------------------------------------------------*/
    private bool AttemptRollback(string fullPath) {
        bool success = false;
        string backupFilePath = fullPath + backupExtension;
        try {
            // if the file exists, attempt to roll back to it by overwriting the original file
            if (File.Exists(backupFilePath)) {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning("Had to roll back to backup file at: " + backupFilePath);
            }
            // otherwise, we don't yet have a backup file - so there's nothing to roll back to
            else {
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        }
        catch (Exception e) {
            Debug.LogError("Error occured when trying to roll back to backup file at: "
                + backupFilePath + "\n" + e);
        }

        return success;
    }
}