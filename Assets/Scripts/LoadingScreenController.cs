using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    /*public SettingsManager settings;
    public bool autoFindSettings;*/

    public RawImage black;

    public float waitingTime;
    public float fadeSpeed;
    private Vector4 blackColors;

    private float timer;

    //public AudioListener playerListener;
    //public bool autoFindPlayer;

    void Start()
    {
        /*if(autoFindSettings)
        {
            settings = GameObject.FindGameObjectWithTag("Settings").gameObject.GetComponent<SettingsManager>();
        }*/

        AudioListener.volume = 0.0f;

        blackColors = black.color;

        /*if(settings.loadGame)
        {
            timer = waitingTime + 1.0f;
        }
        else
        {*/
        timer = 0.0f;
        //}
    }

    void LateUpdate()
    {
        if(timer < waitingTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            if(blackColors.w > 0.0f)
            {
                AudioListener.volume = 1.0f - blackColors.w;
                black.color = new Vector4(blackColors.x, blackColors.y, blackColors.z, blackColors.w - (Time.deltaTime * fadeSpeed));
                blackColors = black.color;
            }
        }
    }

    public bool IsStartingBlackScreenEnded()
    {
        if(timer < waitingTime)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
