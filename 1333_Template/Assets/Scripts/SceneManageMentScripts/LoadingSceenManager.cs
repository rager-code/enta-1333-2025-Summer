using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceenManager : MonoBehaviour
{
    public static LoadingSceenManager instance;
    public GameObject loadingScreenObject;
    public Slider ProgressBar;

    // Make sure there's only one loading screen manager in the game
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Start loading a new scene and show the loading screen
    public void SwitchToScene(int id)
    {
        loadingScreenObject.SetActive(true);
        ProgressBar.value = 0;
        StartCoroutine(SwitchToSceneAsyc(id));
    }

    // Hide the loading screen when we first start up
    void Start()
    {
        loadingScreenObject.SetActive(false);
    }

    // Load the scene in the background and update the progress bar
    IEnumerator SwitchToSceneAsyc(int id)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
        while (!asyncLoad.isDone)
        {
            ProgressBar.value = asyncLoad.progress;
            yield return null;
        }
        // Wait a little bit extra so people can see it finished loading
        yield return new WaitForSeconds(0.5f);
        loadingScreenObject.SetActive(false);
    }
}