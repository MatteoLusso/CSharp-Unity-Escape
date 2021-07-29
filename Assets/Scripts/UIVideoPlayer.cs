using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class UIVideoPlayer : MonoBehaviour
{
    private double clipTime;
    private double currentTime;
    private bool introEnded;

    void Awake()
    {
        introEnded = false;
    }

    void Start ()
    {
        clipTime = this.GetComponent<VideoPlayer>().clip.length;
        this.GetComponent<VideoPlayer>().Play();
    }
 
   
    void LateUpdate ()
    {
        currentTime = this.GetComponent<VideoPlayer>().time;

        if (currentTime >= clipTime)
        {
            introEnded = true;
            foreach(Transform child in GameObject.FindGameObjectWithTag("MenuElement").gameObject.transform)
            {
                child.gameObject.SetActive(true);
            }

            this.gameObject.SetActive(false);
        }
    }

    public bool isIntroEnded()
    {
        return introEnded;
    }
}
