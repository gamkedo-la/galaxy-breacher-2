using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    public void LoadStage(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
