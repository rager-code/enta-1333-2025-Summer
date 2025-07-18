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
    public void SwitchToScene(int id)
    {
        loadingScreenObject.SetActive(true);
        ProgressBar.value = 0;
        StartCoroutine(SwitchToSceneAsyc(id));
    }

    // Start is called before the first frame update
    void Start()
    {
        loadingScreenObject.SetActive(false);
    }

   IEnumerator SwitchToSceneAsyc(int id)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
        while (!asyncLoad.isDone)
        {
            ProgressBar.value = asyncLoad.progress;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        loadingScreenObject.SetActive(false);
    }
}
