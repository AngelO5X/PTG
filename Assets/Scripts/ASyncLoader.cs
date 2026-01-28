using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private Slider loadingSlider;

    public void LoadLevelBtn(string levelToLoad)
    {
        MainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        MapStatus.IsMapReady = false;

        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    IEnumerator LoadLevelASync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = progressValue;

            if (loadOperation.progress >= 0.9f)
            {
                if (MapStatus.IsMapReady)
                {
                    loadOperation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}