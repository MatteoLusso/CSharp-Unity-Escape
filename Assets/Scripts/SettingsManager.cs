using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private GameData checkData;
    public bool loadGame;
    public float blocksDensity;
    public float roomsDensity;
    public float objectsDensity;
    public float enemiesDensity;

    public Slider levelSlider;
    public Slider roomsSlider;
    public Slider objectsSlider;
    public Slider enemiesSlider;

    private static GameObject instance;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
        {
            instance = this.gameObject;
        }
        else
        {
            Destroy(this.gameObject);
        }

        loadGame = false;
        checkData = null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(checkData == null)
        {
            checkData = SaveSystem.LoadGame();
        }

        if(levelSlider != null && roomsSlider != null &&  objectsSlider != null && enemiesSlider != null)
        {
            blocksDensity = (levelSlider.value * 40.0f) + 50.0f; // Il giocatore può importare la densità dei blocchi da 50 a 90,
            roomsDensity = (roomsSlider.value * 65.0f) + 25.0f; // quella delle stanze da 25 a 90 e
            objectsDensity = (objectsSlider.value * 20.0f) + 10.0f; // quella degli oggetti con rigidbody fra 10 e 30;

            enemiesDensity = (1.0f - enemiesSlider.value) * 9; // il giocatore può impostare il numero massimo di nemici fra 1 e 10;
        }
    }

    public bool SaveGameExits()
    {
        if(checkData == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
