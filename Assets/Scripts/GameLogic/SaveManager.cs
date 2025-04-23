using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    public static void SavePlayer(Player player, PowerupManager powerupManager, HealthTracker healthTracker)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.save";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerSaveData data = new PlayerSaveData(player, powerupManager, healthTracker);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Data saved successfuly");
    }

    public static PlayerSaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.save";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerSaveData data = formatter.Deserialize(stream) as PlayerSaveData;
            stream.Close();

            return data;

        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }

    public static void DeletePlayerSave()
    {
        string path = Application.persistentDataPath + "/player.save";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file deleted");
        }
        else
        {
            Debug.Log("No save file found to delete at " + path);
        }
    }



}
