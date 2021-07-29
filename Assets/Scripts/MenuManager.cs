using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{


    public SettingsManager settings;
    public bool autoFindSettingsManager;
    /*public LoadManager loadManager;
    public bool autoFindLoadManager;*/
    public Text pressAnyKeyText;
    public RawImage blackScreen;
    public UIVideoPlayer videoPlayer;
    public LightsManager lightsManager;

    public Slider levelExtension;
    public Slider roomExtension;
    public Slider objectsDensity;
    public Slider enemiesDensity;
    public Text controlsList;
    public Button controls;
    public Button options;
    public Button newGame;
    public Button loadGame;
    public Button exitGame;
    public Button back;

    public GameObject screen;
    public Canvas canvas;


    private bool waitForInput;

    public Slider loadingSlider;
    public Text loadingText;
    public string sceneName;

    public bool firstlevel;
    public bool secondlevel;
    public bool thirdlevel;

    public AudioSource[] noiseSources;
    [Range(0.0f, 1.0f)]
    public float noiseMaxVolume;

    void Awake()
    {
        foreach(AudioSource noise in noiseSources)
        {
            noise.volume = 0.0f;
        }
        waitForInput = true;
        pressAnyKeyText.gameObject.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        lightsManager.LightsOff(true);

        firstlevel = true;
        secondlevel = false;
        thirdlevel = false;
    }

    void Start()
    {
        /*if(autoFindLoadManager)
        {
            loadManager = GameObject.FindGameObjectWithTag("Load Manager").GetComponent<LoadManager>();
        }*/
        if(autoFindSettingsManager)
        {
            settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<SettingsManager>();
        }
    }
    void LateUpdate()
    {
        if(waitForInput)
        {
            if(videoPlayer.isIntroEnded())
            {
                foreach(AudioSource noise in noiseSources)
                {
                    noise.volume = noiseMaxVolume;
                }
                pressAnyKeyText.gameObject.SetActive(true);
                blackScreen.gameObject.SetActive(false);

                if(Input.anyKeyDown)
                {
                    waitForInput = false;
                    pressAnyKeyText.gameObject.SetActive(false);
                    lightsManager.LightsOff(false);
                }
            }
        }
        else
        {
            if(firstlevel)
            {
                canvas.gameObject.SetActive(true);
                newGame.gameObject.SetActive(true);
                loadGame.gameObject.SetActive(true);
                options.gameObject.SetActive(true);
                exitGame.gameObject.SetActive(true);
            }


            screen.SetActive(true);
        }
    }

    public void EnterOptions()
    {
        firstlevel = false;
        secondlevel = true;
        thirdlevel = false;

        newGame.gameObject.SetActive(false);
        loadGame.gameObject.SetActive(false);
        options.gameObject.SetActive(false);
        exitGame.gameObject.SetActive(false);

        controls.gameObject.SetActive(true);
        back.gameObject.SetActive(true);
        
        levelExtension.gameObject.SetActive(true);
        roomExtension.gameObject.SetActive(true);
        objectsDensity.gameObject.SetActive(true);
        enemiesDensity.gameObject.SetActive(true);

        controlsList.gameObject.SetActive(false);
    }

    public void EnterControls()
    {
        firstlevel = false;
        secondlevel = false;
        thirdlevel = true;

        controls.gameObject.SetActive(false);
        
        levelExtension.gameObject.SetActive(false);
        roomExtension.gameObject.SetActive(false);
        objectsDensity.gameObject.SetActive(false);
        enemiesDensity.gameObject.SetActive(false);

        controlsList.gameObject.SetActive(true);
    }

    public void ReturnBack()
    {
        if(thirdlevel)
        {
            EnterOptions();
        }
        else if(secondlevel)
        {
            firstlevel = true;
            secondlevel = false;
            thirdlevel = false;

            newGame.gameObject.SetActive(true);
            loadGame.gameObject.SetActive(true);
            options.gameObject.SetActive(true);
            exitGame.gameObject.SetActive(true);

            controls.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
        
            levelExtension.gameObject.SetActive(false);
            roomExtension.gameObject.SetActive(false);
            objectsDensity.gameObject.SetActive(false);
            enemiesDensity.gameObject.SetActive(false);

            controlsList.gameObject.SetActive(false);
        }
    }

    private void SetUpLoading()
    {
        foreach(AudioSource noise in noiseSources)
        {
            noise.volume = 0.0f;
        }

        loadingSlider.gameObject.SetActive(true);
        loadingSlider.value = 0.0f;

        loadingText.gameObject.SetActive(true);

        blackScreen.gameObject.SetActive(true);

        StartCoroutine(LoadNewScene(sceneName));
    }


    public void LoadGame()
    {
        if(settings.SaveGameExits())
        {
            settings.loadGame = true;
            SetUpLoading();
        }
    }

    public void StartGame()
    {
        settings.loadGame = false;
        SetUpLoading();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public bool isMenuVisible()
    {
        return !waitForInput;
    }

    IEnumerator LoadNewScene(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        while (!async.isDone)
        {
            float progress = async.progress / 0.9f;
            loadingSlider.value = progress;

            //Debug.Log(progress + " " + async.progress);
            yield return new WaitForEndOfFrame();
        } 
    }

}
