using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Cpp;

/// <summary>
/// Class which manages the game
/// </summary>
public class GameManager : MonoBehaviour
{
    public bool isServer = false;
    // The script that manages all others
    public static GameManager instance = null;

    [Tooltip("The UIManager component which manages the current scene's UI")]
    public UIManager uiManager = null;

    [Tooltip("The player gameobject")]
    public GameObject player = null;

    [Header("Scores")]
    // The current player score in the game
    [Tooltip("The player's score")]
    [SerializeField] private static int gameManagerScore = 0;

    // Static getter/setter for player score (for convenience)
    public static int score
    {
        get
        {
            return gameManagerScore;
        }
        set
        {
            gameManagerScore = value;
        }
    }

    public string playerName = "";
    public string serverAddress = NetConstants.defaultServerAddress;
    public int serverPort = NetConstants.defaultServerPort;
    public int port = NetConstants.defaultServerPort;
    public int botCount = 0;
    public int maxPlayers = 10;
    public int aiControlled = 0;

    // The highest score obtained by this player
    [Tooltip("The highest score acheived on this device")]
    public int highScore = 0;

    [Header("Game Progress / Victory Settings")]
    [Tooltip("Whether the game is winnable or not \nDefault: true")]
    public bool gameIsWinnable = true;
    [Tooltip("The number of enemies that must be defeated to win the game")]
    public int enemiesToDefeat = 10;
    
    // The number of enemies defeated in game
    private int enemiesDefeated = 0;

    [Tooltip("Whether or not to print debug statements about whether the game can be won or not according to the game manager's" +
        " search at start up")]
    public bool printDebugOfWinnableStatus = true;
    [Tooltip("Page index in the UIManager to go to on winning the game")]
    public int gameVictoryPageIndex = 0;
    [Tooltip("The effect to create upon winning the game")]
    public GameObject victoryEffect;

    //The number of enemies observed by the game manager in this scene at start up"
    private int numberOfEnemiesFoundAtStart;

    public int chosenShip = 0;
    public List<GameObject> ships;
    public List<GameObject> multiplayerShips;
    public List<GameObject> multiplayerBotShips;

    public string selectedLevel = "0";

    /// <summary>
    /// Description:
    /// Standard Unity function called when the script is loaded, called before start
    /// 
    /// When this component is first added or activated, setup the global reference
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Awake()
    {
        Application.runInBackground = true; // TODO: Remove

        if (instance == null)
        {
            instance = this;
            
            if (PlayerPrefs.HasKey("player_name"))
            {
                playerName = PlayerPrefs.GetString("player_name", "Player");
                Debug.Log("Player name: " + playerName);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("PlayerName").GetComponent<InputFieldGrabber>().SetInputField(playerName);
                }
            }
            if (PlayerPrefs.HasKey("server_address"))
            {
                serverAddress = PlayerPrefs.GetString("server_address", NetConstants.defaultServerAddress);
                Debug.Log("Server address: " + serverAddress);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("ServerAddress").GetComponent<InputFieldGrabber>().SetInputField(serverAddress);
                }
            }
            if (PlayerPrefs.HasKey("server_port"))
            {
                serverPort = PlayerPrefs.GetInt("server_port", NetConstants.defaultServerPort);
                Debug.Log("Server Port: " + serverPort);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("ServerPort").GetComponent<InputFieldGrabber>().SetInputField(serverPort.ToString());
                }
            }
            if (PlayerPrefs.HasKey("port"))
            {
                port = PlayerPrefs.GetInt("port", NetConstants.defaultServerPort);
                Debug.Log("Port: " + port);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("Port").GetComponent<InputFieldGrabber>().SetInputField(port.ToString());
                }
            }
            if (PlayerPrefs.HasKey("bot_count"))
            {
                botCount = PlayerPrefs.GetInt("bot_count", 0);
                Debug.Log("Bot Count: " + botCount);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("BotCount").GetComponent<InputFieldGrabber>().SetInputField(botCount.ToString());
                }
            }
            if (PlayerPrefs.HasKey("max_players"))
            {
                maxPlayers = PlayerPrefs.GetInt("max_players", 10);
                Debug.Log("Max Players: " + maxPlayers);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("MaxPlayers").GetComponent<InputFieldGrabber>().SetInputField(maxPlayers.ToString());
                }
            }
            if (PlayerPrefs.HasKey("ai_control"))
            {
                aiControlled = PlayerPrefs.GetInt("ai_control");
                Debug.Log($"ai_control: {aiControlled}");
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("AIControl").GetComponent<ToggleScript>().SetInitialValue(aiControlled);
                }
            }
            if (PlayerPrefs.HasKey("highscore"))
            {
                highScore = PlayerPrefs.GetInt("highscore");
            }
            if (PlayerPrefs.HasKey("score"))
            {
                score = PlayerPrefs.GetInt("score");
            }

            if (SceneManager.GetActiveScene().name == "LevelMultiplayerClient")
            {
                chosenShip = PlayerPrefs.GetInt("ship");
                GameObject player;
                if (aiControlled != 0)
                {
                    player = Instantiate(multiplayerBotShips[chosenShip]);
                }
                else
                {
                    player = Instantiate(multiplayerShips[chosenShip]);

                    player.GetComponentsInChildren<ShootingController>();
                }
                instance.player = player;
                GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<CameraController>().target = player.transform;
            }
            else if (SceneManager.GetActiveScene().name == "LevelMultiplayerServer")
            {
                
            }
            else if (SceneManager.GetActiveScene().name == "LevelUnityMultiplayer")
            {
                
            }
            else if ((SceneManager.GetActiveScene().name != "MainMenu") && !isServer)
            {
                chosenShip = PlayerPrefs.GetInt("ship");
                GameObject player = Instantiate(ships[chosenShip]);
                instance.player = player;
                GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<CameraController>().target = player.transform;
            }
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        HandleStartUp();
    
#if UNITY_SERVER
        Debug.Log("Standalone Windows");
        SceneManager.LoadScene("LevelMultiplayerServer");
#endif
    }

    /// <summary>
    /// Description:
    /// Handles necessary activities on start up such as getting the highscore and score, updating UI elements, 
    /// and checking the number of enemies
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void HandleStartUp()
    {
        if (isServer)
        {
            // uiManager.;
        }
        else
        {
            UpdateUIElements();
        }

        if (printDebugOfWinnableStatus)
        {
            FigureOutHowManyEnemiesExist();
        }
    }

    /// <summary>
    /// Description:
    /// Searches the level for all spawners and static enemies.
    /// Only produces debug messages / warnings if the game is set to be winnable
    /// If there are any infinite spawners a debug message will say so,
    /// If there are more enemies than the number of enemies to defeat to win
    /// then a debug message will say so
    /// If there are too few enemies to defeat to win then a debug warning will say so
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void FigureOutHowManyEnemiesExist()
    {
        List<EnemySpawner> enemySpawners = FindObjectsOfType<EnemySpawner>().ToList();
        List<Enemy> staticEnemies = FindObjectsOfType<Enemy>().ToList();

        int numberOfInfiniteSpawners = 0;
        int enemiesFromSpawners = 0;
        int enemiesFromStatic = staticEnemies.Count;
        foreach(EnemySpawner enemySpawner in enemySpawners)
        {
            if (enemySpawner.spawnInfinite)
            {
                numberOfInfiniteSpawners += 1;
            }
            else
            {
                enemiesFromSpawners += enemySpawner.maxSpawn;
            }
        }
        numberOfEnemiesFoundAtStart = enemiesFromSpawners + enemiesFromStatic;

        if (gameIsWinnable)
        {
            if (numberOfInfiniteSpawners > 0)
            {
                Debug.Log("There are " + numberOfInfiniteSpawners + " infinite spawners " + " so the level will always be winnable, "
                    + "\nhowever you sshould still playtest for timely completion");
            }
            else if (!SceneManager.GetActiveScene().name.Contains("Multiplayer") && (enemiesToDefeat > numberOfEnemiesFoundAtStart))
            {
                Debug.LogWarning("There are " + enemiesToDefeat + " enemies to defeat but only " + numberOfEnemiesFoundAtStart + 
                    " enemies found at start \nThe level can not be completed!");
            }
            else
            {
                Debug.Log("There are " + enemiesToDefeat + " enemies to defeat and " + numberOfEnemiesFoundAtStart +
                    " enemies found at start \nThe level can completed");
            }
        }
    }

    /// <summary>
    /// Description:
    /// Increments the number of enemies defeated by 1
    /// Input:
    /// none
    /// Return:
    /// void (no returned value)
    /// </summary>
    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
        if (enemiesDefeated >= enemiesToDefeat && gameIsWinnable)
        {
            LevelCleared();
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function that gets called when the application (or playmode) ends
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveHighScore();
        ResetScore();
    }

    /// <summary>
    /// Description:
    /// Adds a number to the player's score stored in the gameManager
    /// Input: 
    /// int scoreAmount
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="scoreAmount">The amount to add to the score</param>
    public static void AddScore(int scoreAmount)
    {
        score += scoreAmount;
        if (score > instance.highScore)
        {
            SaveHighScore();
        }
        UpdateUIElements();
    }
    
    /// <summary>
    /// Description:
    /// Resets the current player score
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public static void ResetScore()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
    }

    /// <summary>
    /// Description:
    /// Saves the player's highscore
    /// Input: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public static void SaveHighScore()
    {
        if (score > instance.highScore)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }
        UpdateUIElements();
    }

    public void SavePref(string pref, string value)
    {
        switch (pref)
        {
            case "player_name":
            {
                instance.playerName = value;
                PlayerPrefs.SetString(pref, instance.playerName);
                break;
            }
            case "server_address":
            {
                instance.serverAddress = value;
                PlayerPrefs.SetString(pref, instance.serverAddress);
                break;
            }
            case "server_port":
            {
                if (!int.TryParse(value, out instance.serverPort))
                {
                    instance.serverPort = NetConstants.defaultServerPort;
                } 
                PlayerPrefs.SetInt(pref, instance.serverPort);
                break;
            }
            case "port":
            {
                if (!int.TryParse(value, out instance.port))
                {
                    instance.port = NetConstants.defaultServerPort;
                } 
                PlayerPrefs.SetInt(pref, instance.port);
                break;
            }
            case "bot_count":
            {
                if (!int.TryParse(value, out instance.botCount))
                {
                    instance.botCount = 0;
                } 
                PlayerPrefs.SetInt(pref, instance.botCount);
                break;
            }
            case "max_players":
            {
                if (!int.TryParse(value, out instance.maxPlayers))
                {
                    instance.maxPlayers = 10;
                } 
                PlayerPrefs.SetInt(pref, instance.maxPlayers);
                break;
            }
            default:
            {
                break;
            }
        }
    }

    /// <summary>
    /// Description:
    /// Resets the high score in player preferences
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public static void ResetHighScore()
    {
        PlayerPrefs.SetInt("highscore", 0);
        if (instance != null)
        {
            instance.highScore = 0;
        }
        UpdateUIElements();
    }

    /// <summary>
    /// Description:
    /// Sends out a message to UI elements to update
    /// Input: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public static void UpdateUIElements()
    {
        if (instance != null && instance.uiManager != null)
        {
            instance.uiManager.UpdateUI();
        }
    }

    /// <summary>
    /// Description:
    /// Ends the level, meant to be called when the level is complete (enough enemies have been defeated)
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public void LevelCleared()
    {
        PlayerPrefs.SetInt("score", score);
        if (uiManager != null)
        {
            player.SetActive(false);
            uiManager.allowPause = false;
            uiManager.GoToPage(gameVictoryPageIndex);
            if (victoryEffect != null)
            {
                Instantiate(victoryEffect, transform.position, transform.rotation, null);
            }
        }     
    }

    [Header("Game Over Settings:")]
    [Tooltip("The index in the UI manager of the game over page")]
    public int gameOverPageIndex = 0;
    [Tooltip("The game over effect to create when the game is lost")]
    public GameObject gameOverEffect;

    // Whether or not the game is over
    [HideInInspector]
    public bool gameIsOver = false;

    /// <summary>
    /// Description:
    /// Displays game over screen
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public void GameOver()
    {
        gameIsOver = true;
        if (gameOverEffect != null)
        {
            Instantiate(gameOverEffect, transform.position, transform.rotation, null);
        }
        if (uiManager != null)
        {
            uiManager.allowPause = false;
            uiManager.GoToPage(gameOverPageIndex);
        }
    }

    public static void SetShip(int shipIndex)
    {
        PlayerPrefs.SetInt("ship", shipIndex);
    }

    public void SetLevel(string level)
    {
        selectedLevel = level;
    }

    public void LoadSelectedLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Level" + selectedLevel);
    }
}
