using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void StartLoadingLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
