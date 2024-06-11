using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceSelector : MonoBehaviour
{
    public void SelectOrc()
    {
        PlayerPrefs.SetInt("SelectedRace", 1);
        LoadGameScene();
    }

    public void SelectElf()
    {
        PlayerPrefs.SetInt("SelectedRace", 4);
        LoadGameScene();
    }

    public void SelectHuman()
    {
        PlayerPrefs.SetInt("SelectedRace", 2);
        LoadGameScene();
    }

    public void SelectUndead()
    {
        PlayerPrefs.SetInt("SelectedRace", 3);
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
