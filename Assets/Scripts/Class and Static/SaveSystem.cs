using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{

    public static void SaveGame(GameData game_Data)
    {
        BinaryFormatter savegame_Formatter = new BinaryFormatter();
        string savegame_Path = Application.persistentDataPath + ".save";

        FileStream savegame_Stream = new FileStream(savegame_Path, FileMode.Create);

        savegame_Formatter.Serialize(savegame_Stream, game_Data);
        savegame_Stream.Close();

        //Debug.Log(savegame_Path);
    }

    public static GameData LoadGame()
    {
        string savegame_Path = Application.persistentDataPath + ".save";

        if(File.Exists(savegame_Path))
        {
            BinaryFormatter savegame_Formatter = new BinaryFormatter();
            FileStream savegame_Stream = new FileStream(savegame_Path, FileMode.Open);
            GameData level_Data = savegame_Formatter.Deserialize(savegame_Stream) as GameData;
            savegame_Stream.Close();

            return level_Data;
        }
        else
        {
            Debug.Log("Salvataggio non trovato");
            return null;
        }

    }

    // Ho adattato lo script di Brackeys https://www.youtube.com/watch?v=XOjd_qU2Ido

}


