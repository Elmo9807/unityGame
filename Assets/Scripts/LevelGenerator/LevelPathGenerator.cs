using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using static UnityEditor.PlayerSettings;
#endif
using JetBrains.Annotations;

public class LevelPathGenerator : MonoBehaviour
{
    public RoomTemplate startRoom;
    public RoomTemplate bossRoom;
    public RoomTemplate shopRoom;



    public int minRooms = 5;
    public int maxRooms = 10;
    public int maxSideRooms = 3;


    public Vector2 roomSize = new Vector2(30, 20);
    public Vector2Int startRoomPosition = Vector2Int.zero;

    private List<Vector2Int> mainRooms = new List<Vector2Int>();
    private List<Vector2Int> sideRooms = new List<Vector2Int>();

    public void GeneratePath()
    {
        mainRooms.Clear();
        sideRooms.Clear();


        Vector2Int currentPos = startRoomPosition;

        
        mainRooms.Add(currentPos);


        currentPos += Vector2Int.right;
        mainRooms.Add(currentPos);



        int roomCount = Random.Range(minRooms, maxRooms + 1);
        for (int i = 1; i < roomCount; i++)
        {
            currentPos = GetNextRoomPosition(currentPos);
            mainRooms.Add(currentPos);
        }

        
        currentPos += Vector2Int.right;
        mainRooms.Add(currentPos);


        currentPos += Vector2Int.right;
        mainRooms.Add(currentPos);

        GenerateSidePath();
    }

    private Vector2Int GetNextRoomPosition(Vector2Int currentPos)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>
        {
            new Vector2Int(currentPos.x + 1, currentPos.y), // Right
            new Vector2Int(currentPos.x, currentPos.y + 1), // Up
            new Vector2Int(currentPos.x, currentPos.y - 1)  // Down
        };

        // Remove positions that already exist to prevent loops
        possibleMoves.RemoveAll(pos => mainRooms.Contains(pos));

        if (possibleMoves.Count == 0)
        {
            return currentPos; // If no move is possible, stay in the same position
        }

        return possibleMoves.Count > 0 ? possibleMoves[Random.Range(0, possibleMoves.Count)] : currentPos;
    }

    private void GenerateSidePath()
    {
        int sideRoomCount = Random.Range(1, maxSideRooms);
        for (int i = 0; i < sideRoomCount; i++)
        {
            int attachIndex = Random.Range(1, mainRooms.Count - 2); //don't attach to spawn, shop and boss
            Vector2Int baseRoom = mainRooms[attachIndex];

            List<Vector2Int> possibleMoves = new List<Vector2Int>
            {
                baseRoom + Vector2Int.up,
                baseRoom + Vector2Int.down,
                baseRoom + Vector2Int.left
            };

            possibleMoves.RemoveAll(pos => mainRooms.Contains(pos) || sideRooms.Contains(pos));

            if (possibleMoves.Count > 0)
            {
                Vector2Int sideRoom = possibleMoves[Random.Range(0, possibleMoves.Count)];
                sideRooms.Add(sideRoom);
            }
        }

    }

    public List<Vector2Int> GetMainPath() => mainRooms;
    public List<Vector2Int> GetSideRooms() => sideRooms;
    public RoomTemplate GetStartRoom() => startRoom;
    public RoomTemplate GetShopRoom() => shopRoom;
    public RoomTemplate GetBossRoom() => bossRoom;


    /* Gizmos to be viewed in the scene for debugginig, I removed it cause it is annoying */

    private void OnDrawGizmos()
    {
        if (mainRooms.Count == 0)
        {
            GeneratePath();
        }

        for (int i = 0; i < mainRooms.Count; i++)
        {
            Vector3 roomPos = new Vector3(mainRooms[i].x * roomSize.x, mainRooms[i].y * roomSize.y, 0);

            if (i == 0)
                Gizmos.color = Color.green; // Start Room
            else if (i == mainRooms.Count - 2)
                Gizmos.color = Color.white; // Shop Room
            else if (i == mainRooms.Count - 1)
                Gizmos.color = Color.red; // Boss Room
            else
                Gizmos.color = Color.blue; // Normal Rooms

            Gizmos.DrawCube(roomPos, new Vector3(2, 2, 2));

            if (i > 0)
            {
                Vector3 prevRoomPos = new Vector3(mainRooms[i - 1].x * roomSize.x, mainRooms[i - 1].y * roomSize.y, 0);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(prevRoomPos, roomPos);
            }
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < sideRooms.Count; i++)
        {
            Vector3 roomPos = new Vector3(sideRooms[i].x * roomSize.x, sideRooms[i].y * roomSize.y, 0);
            Gizmos.DrawCube(roomPos, new Vector3(2, 2, 2));
        }
    }
}
