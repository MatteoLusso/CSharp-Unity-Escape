using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{

    public GameObject player;
    public bool autoFindPlayer;
    public MazeGenerator level;
    public PadController pad;
    private GameData save_Info;

    private PodController aPod;

    private GameObject[] enemies;

    private void Start()
    {
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        enemies = new GameObject[GameObject.FindGameObjectsWithTag("Enemy").Length];

        for(int i = 0; i < GameObject.FindGameObjectsWithTag("Enemy").Length; i++)
        {
            enemies[i] = GameObject.FindGameObjectsWithTag("Enemy")[i];
        }

        aPod = GameObject.FindGameObjectWithTag("Pod").GetComponent<PodController>();
    }

    /*private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Save();
        }
        
    }*/

    public void Save()
    {
        save_Info = new GameData(level, player, pad, enemies, aPod);

        SaveSystem.SaveGame(save_Info);

        //Debug.Log("Salvataggio eseguito, seed = " + save_Info.level_Seed);
    }
}
