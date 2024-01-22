using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Create a singleton instance
    private static GameManager _instance; 
    public HashSet<Vector3Int> ModifiedCellTiles;
    public HashSet<Vector3> ModifiedWorldTiles;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Game Manager is null.");
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            ModifiedCellTiles = new HashSet<Vector3Int>();
            ModifiedWorldTiles = new HashSet<Vector3>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Demo");
    }
}
