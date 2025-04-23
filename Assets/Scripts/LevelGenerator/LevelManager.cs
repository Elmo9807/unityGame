using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelPathGenerator pathGenerator;
    public RoomSpawner roomSpawner;

    private List<Vector2Int> mainRooms;
    private List<Vector2Int> sideRooms;

    private void Start()
    {
        GenerateRooms();
    }

    private void GenerateRooms()
    {

        if (pathGenerator != null && roomSpawner != null)
        {
            clearLevel();

            pathGenerator.GeneratePath();

            mainRooms = pathGenerator.GetMainPath();
            sideRooms = pathGenerator.GetSideRooms();
            RoomTemplate startRoom = pathGenerator.GetStartRoom();
            RoomTemplate shopRoom = pathGenerator.GetShopRoom();
            RoomTemplate bossRoom = pathGenerator.GetBossRoom();

            roomSpawner.SpawnRooms(mainRooms, sideRooms, startRoom, shopRoom, bossRoom);
        }
    }

    private void clearLevel()
    {
        foreach (Transform room in roomSpawner.roomParent)
        {
            Destroy(room.gameObject);
        }
    }
}

