using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public PadController pad;
    public LightsManager lights;

    //private List<PodController> pods;

    private GameData save_Info;
    private bool loading;

    //private static GameObject instance;

    /*private void Start()
    {
        foreach(GameObject pod in GameObject.FindGameObjectsWithTag("Pod"))
        {
            pod.GetComponent<PodController>();
        }
    }*/

    public void Load()
    {
        save_Info = SaveSystem.LoadGame();

        //Debug.Log(save_Info.level_Seed);

        if(save_Info != null)
        {
            loading = true;

            //Debug.Log("Caricamento dati eseguito, seed = " + save_Info.level_Seed);
            //Debug.Log("Stato energia ausiliaria: " + save_Info.pad_AuxiliaryEnergyActive);
            //Debug.Log("Stato corrente deviata: " + save_Info.isPowerDeviate);
            //Debug.Log("Stato mappa: " + save_Info.pad_MapActive);

            lights.LightsOff(save_Info.isPowerDeviate);
            //Debug.Log("La corrente è stata deviata? " + save_Info.isPowerDeviate);
            pad.ForceAuxiliaryEnergyStatusTo(save_Info.pad_AuxiliaryEnergyActive);
            //Debug.Log("Lo stato della mappa è stato impostato su: " + save_Info.pad_MapActive);

            /*if(save_Info.isPowerDeviate)
            {
                foreach(PodController pod in pods)
                {
                    pod.UnlockPodDoor();
                }
            }*/
        }
    }

    public bool IsLoading()
    {
        return loading;
    }

    public void SetLoading(bool status)
    {
        loading = status;
    }

    public GameData GetSavedData()
    {
        return save_Info;
    }
}
