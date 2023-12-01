using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public string sceneName;
    public bool loadOnStart = false;
    // Start is called before the first frame update
    void Start()
    {
        if (loadOnStart)
        {
            Load();
        }
    }
    
    public void Load()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
