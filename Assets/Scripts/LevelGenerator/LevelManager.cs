using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelPathGenerator pathGenerator;
    public RoomSpawner roomSpawner;

    private void Start()
    {
        if (pathGenerator != null && roomSpawner != null)
        {
            pathGenerator.GeneratePath();
            List<Vector2Int> path = pathGenerator.GetPath();
            RoomTemplate startRoom = pathGenerator.GetStartRoom();
            RoomTemplate shopRoom = pathGenerator.GetShopRoom();
            RoomTemplate bossRoom = pathGenerator.GetBossRoom();

            roomSpawner.SpawnRooms(path, startRoom, shopRoom, bossRoom);
        }
    }
}

