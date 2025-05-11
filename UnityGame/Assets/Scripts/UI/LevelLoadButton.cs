using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;


/// <summary>
/// This class is meant to be used on buttons as a quick easy way to load levels (scenes)
/// </summary>
public class LevelLoadButton : MonoBehaviour
{
    /// <summary>
    /// Description:
    /// Loads a level according to the name provided
    /// Input:
    /// string levelToLoadName
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="levelToLoadName">The name of the level to load</param>
    public void LoadLevelByName(string levelToLoadName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelToLoadName);
    }

    public void LoadLevelByNameNetcode(string levelToLoadName)
    {
        GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerUI>().takenPlayerNameMap = new ConcurrentDictionary<FixedString64Bytes, ushort>();
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        Time.timeScale = 1;
        SceneManager.LoadScene(levelToLoadName);
    }
}
