using UnityEngine;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    public List<RoomTemplate> roomTemplates;
    public Transform roomParent;
    private Dictionary<Vector2Int, GameObject> spawnedRooms = new Dictionary<Vector2Int, GameObject>();

    private LevelPathGenerator pathGenerator;

    public EnemySpawner enemySpawner;

    private void Start()
    {
        pathGenerator = FindFirstObjectByType<LevelPathGenerator>();
        if (pathGenerator != null)
        {
            SpawnRooms(pathGenerator.GetPath(), pathGenerator.GetStartRoom(), pathGenerator.GetShopRoom(), pathGenerator.GetBossRoom());
        }
    }


    public void SpawnRooms(List<Vector2Int> path, RoomTemplate startRoom, RoomTemplate shopRoom, RoomTemplate bossRoom)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int position = path[i];
            Vector2Int? previousPosition = (i > 0) ? path[i - 1] : (Vector2Int?)null;
            Vector2Int? nextPosition = (i < path.Count - 1) ? path[i + 1] : (Vector2Int?)null;

            if (spawnedRooms.ContainsKey(position))
            {
                continue;
            }

            RoomTemplate chosenRoom;
            if (i == 0)
            {
                chosenRoom = startRoom;
            }
            else if (i == path.Count - 2)
            {
                chosenRoom = shopRoom;
            }
            else if (i == path.Count - 1)
            {
                chosenRoom = bossRoom;
            }
            else
            {
                chosenRoom = GetMatchingRoom(position, previousPosition, nextPosition);
            }

            if (chosenRoom != null)
            {
                GameObject newRoom = Instantiate(chosenRoom.roomPrefab, new Vector3(position.x * 10, position.y * 10, 0), Quaternion.identity, roomParent);
                spawnedRooms.Add(position, newRoom);
                if (i > 0 && i < path.Count - 2 && enemySpawner != null)
                {
                    enemySpawner.SpawnEnemiesInRoom(newRoom);
                }
            }
        }
    }

    private RoomTemplate GetMatchingRoom(Vector2Int position, Vector2Int? previousPosition, Vector2Int? nextPosition)
    {
        bool needTopExit = false, needBottomExit = false, needLeftExit = false, needRightExit = false;

        if (previousPosition.HasValue)
        {
            Vector2Int dirFromPrevious = position - previousPosition.Value;
            if (dirFromPrevious.y > 0) needBottomExit = true;
            if (dirFromPrevious.y < 0) needTopExit = true;
            if (dirFromPrevious.x > 0) needLeftExit = true;
            if (dirFromPrevious.x < 0) needRightExit = true;
        }


        if (nextPosition.HasValue)
        {
            Vector2Int dirToNext = nextPosition.Value - position;
            if (dirToNext.y > 0) needTopExit = true;
            if (dirToNext.y < 0) needBottomExit = true;
            if (dirToNext.x > 0) needRightExit = true;
            if (dirToNext.x < 0) needLeftExit = true;
        }

        List<RoomTemplate> validRooms = new List<RoomTemplate>();

        foreach (RoomTemplate room in roomTemplates)
        {
            bool isValid = true;

            if (needTopExit && !room.hasTopExit) isValid = false;
            if (needBottomExit && !room.hasBottomExit) isValid = false;
            if (needLeftExit && !room.hasLeftExit) isValid = false;
            if (needRightExit && !room.hasRightExit) isValid = false;

            if (isValid)
            {
                validRooms.Add(room);
            }
        }


        return validRooms.Count > 0 ? validRooms[Random.Range(0, validRooms.Count)] : null;
    }
}

