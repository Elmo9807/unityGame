using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UIElements;

public class RoomSpawner : MonoBehaviour
{
    public List<RoomTemplate> roomTemplates;
    public UnityEngine.Transform roomParent;
    private Dictionary<Vector2Int, GameObject> spawnedRooms = new Dictionary<Vector2Int, GameObject>();

    private LevelPathGenerator pathGenerator;

    public EnemySpawner enemySpawner;

    private void Start()
    {
        pathGenerator = FindFirstObjectByType<LevelPathGenerator>();
        if (pathGenerator != null)
        {
            SpawnRooms(pathGenerator.GetMainPath(), pathGenerator.GetSideRooms(), pathGenerator.GetStartRoom(), pathGenerator.GetShopRoom(), pathGenerator.GetBossRoom());
        }
    }


    public void SpawnRooms(List<Vector2Int> mainPath, List<Vector2Int> sideRooms, RoomTemplate startRoom, RoomTemplate shopRoom, RoomTemplate bossRoom)
    {
        List<Vector2Int> allRooms = mainPath;
        allRooms.AddRange(sideRooms);

        for (int i = 0; i < allRooms.Count; i++)
        {
            Vector2Int position = allRooms[i];
            if (spawnedRooms.ContainsKey(position))
            {
                continue;
            }
            RoomTemplate chosenRoom;
            if (i == 0)
            {
                chosenRoom = startRoom;
            }
            else if (i == allRooms.Count - sideRooms.Count - 2)
            {
                chosenRoom = shopRoom;
            }
            else if (i == allRooms.Count - sideRooms.Count - 1)
            {
                chosenRoom = bossRoom;
            }
            else
            {
                chosenRoom = GetMatchingRoom(position, mainPath, sideRooms);
            }
            if (chosenRoom != null)
            {
                GameObject newRoom = Instantiate(chosenRoom.roomPrefab, new Vector3(position.x * 30, position.y * 20, 0), Quaternion.identity, roomParent);
                spawnedRooms.Add(position, newRoom);
                if (enemySpawner != null && i > 0 && i < allRooms.Count - sideRooms.Count - 2)
                {
                    enemySpawner.SpawnEnemiesInRoom(newRoom);
                }
            }
        }
    }

    private RoomTemplate GetMatchingRoom(Vector2Int position, List<Vector2Int> mainPath, List<Vector2Int> sideRooms)
    {
        bool needTopExit = false, needBottomExit = false, needLeftExit = false, needRightExit = false;

        List<Vector2Int> neighbours = new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right
        };
        

        foreach (var neighbour in neighbours)
        {
            if (mainPath.Contains(neighbour) || sideRooms.Contains(neighbour))
            {
                if (neighbour == position + Vector2Int.up) needTopExit = true;
                if (neighbour == position + Vector2Int.down) needBottomExit = true;
                if (neighbour == position + Vector2Int.left) needLeftExit = true;
                if (neighbour == position + Vector2Int.right) needRightExit = true;
            }
        }

        List<RoomTemplate> validRooms = new List<RoomTemplate>();

        foreach (RoomTemplate room in roomTemplates)
        {

            if(room.hasTopExit == needTopExit &&
                room.hasBottomExit == needBottomExit &&
                room.hasRightExit == needRightExit &&
                room.hasLeftExit == needLeftExit)
            {
                validRooms.Add(room);
            }
        }


        return validRooms.Count > 0 ? validRooms[Random.Range(0, validRooms.Count)] : null;
    }
}

