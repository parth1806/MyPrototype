using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    public GameObject levelCompletedPanel;
    public GameObject pausePanel;

    public TMP_Text scoreText;
    public TMP_Text scoreComboText;

    private int _levelScore;
    private int _gridRows;
    private int _gridColumns;

    // Start is called before the first frame update
    void Start()
    {
        levelCompletedPanel.SetActive(false);
        ShowPausePanel(false);
        string level = PlayerPrefs.GetString("Level");
        LoadLevel(level);
    }

    public void LoadLevel(string gridSizeStr)
    {
        var gridSize = gridSizeStr.Split('x');
        _gridRows = int.Parse(gridSize[0]);
        _gridColumns = int.Parse(gridSize[1]);
        LevelManager.Instance.StartLevel(new Vector2Int(_gridRows, _gridColumns));
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Completed");
        StartCoroutine(WaitForLevelComplete());
    }

    IEnumerator WaitForLevelComplete()
    {
        yield return new WaitForSeconds(1);
        levelCompletedPanel.SetActive(true);
    }

    public void OnClick_Restart()
    {
        DeleteFile();
        SceneManager.LoadScene(1);
    }
    public void OnClick_ModeSelect()
    {
        DeleteFile();
        SceneManager.LoadScene(0);
    }

    public void OnClick_SaveAndHome()
    {
        SceneManager.LoadScene(0);
    }

    public void DeleteFile()
    {
        string fileName = new Vector2Int(_gridRows, _gridColumns).ToString();
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("File deleted: " + path);
        }
        else
        {
            Debug.Log("File not found: " + path);
        }
    }

    public void ScoreAdd(int score)
    {
        _levelScore += score;
        ShowScore();
    }

    public void ShowScore()
    {
        scoreText.text = "Score: " + _levelScore;
    }

    public void ShowScoreCombo(int combo)
    {
        scoreComboText.text = "Combo: " + combo + " X";
    }

    public void ShowPausePanel(bool isActive)
    {
        pausePanel.SetActive(isActive);
    }
}
