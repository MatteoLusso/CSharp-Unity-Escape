using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int level_Seed;

    public float[] player_Position;
    public float[] player_Rotation;

    public bool pad_AuxiliaryEnergyActive;
    public bool pad_MapActive;

    public bool isPowerDeviate;

    public int enemies_Count;

    public float[] enemies_XPositions;
    public float[] enemies_YPositions;
    public float[] enemies_ZPositions;

    public float[] enemies_XRotations;
    public float[] enemies_YRotations;
    public float[] enemies_ZRotations;
    public float[] enemies_WRotations;
    public GameData (MazeGenerator level, GameObject player, PadController pad, GameObject[] enemies, PodController pod)
    {
        level_Seed = level.seed;

        player_Position = new float[3];
        player_Rotation = new float[4];

        player_Position[0] = player.transform.position.x;
        player_Position[1] = player.transform.position.y;
        player_Position[2] = player.transform.position.z;

        player_Rotation[0] = player.transform.rotation.x;
        player_Rotation[1] = player.transform.rotation.y;
        player_Rotation[2] = player.transform.rotation.z;
        player_Rotation[3] = player.transform.rotation.w;

        //----//

        pad_AuxiliaryEnergyActive = pad.AuxiliaryEnergyFound();
        pad_MapActive = pad.IsMapVisible();

        //----//

        isPowerDeviate = pod.IsPodDoorActive();

        //----// !!!!!!!Tanto odio per Unity che non serializza i GameObject!!!!!!!!!

        enemies_Count = enemies.Length;

        enemies_XPositions = new float[enemies_Count];
        enemies_YPositions = new float[enemies_Count];
        enemies_ZPositions = new float[enemies_Count];

        enemies_XRotations = new float[enemies_Count];
        enemies_YRotations = new float[enemies_Count];
        enemies_ZRotations = new float[enemies_Count];
        enemies_WRotations = new float[enemies_Count];

        foreach(GameObject enemy in enemies)
        {
            for(int i = 0; i < enemies_Count; i++)
            {
                enemies_XPositions[i] = enemies[i].transform.position.x;
                enemies_YPositions[i] = enemies[i].transform.position.y;
                enemies_ZPositions[i] = enemies[i].transform.position.z;

                enemies_XRotations[i] = enemies[i].transform.rotation.x;
                enemies_YRotations[i] = enemies[i].transform.rotation.y;
                enemies_ZRotations[i] = enemies[i].transform.rotation.z;
                enemies_WRotations[i] = enemies[i].transform.rotation.w;
            }
        }
    }
}
