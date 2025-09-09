using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelection : MonoBehaviour
{
    public void OnClickLevel(string gridSize)
    {
        PlayerPrefs.SetString("Level", gridSize);
        SceneManager.LoadScene(1);
    }
}
