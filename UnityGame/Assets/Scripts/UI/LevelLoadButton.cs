using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;


public class LevelLoadButton : MonoBehaviour
{
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
