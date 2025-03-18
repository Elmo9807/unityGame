using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class LevelPathGenerator : MonoBehaviour
{
    public RoomTemplate startRoom;
    public RoomTemplate bossRoom;
    public RoomTemplate shopRoom;

    public int minRooms = 5; // Minimum number of rooms before the boss
    public int maxRooms = 10; // Maximum number of rooms before the boss
    public Vector2 roomSize = new Vector2(10, 10); // Size of each room
    public Vector2Int startRoomPosition = Vector2Int.zero; // Starting position of the path

    private List<Vector2Int> roomPositions = new List<Vector2Int>();

    public void GeneratePath()
    {
        roomPositions.Clear();
        Vector2Int currentPos = Vector2Int.zero;

        // Step 1: Place the Start Room at (0,0)
        roomPositions.Add(currentPos);

        // Step 2: Move right from the Start Room
        currentPos += Vector2Int.right;
        roomPositions.Add(currentPos);

        // Step 3: Generate intermediate rooms
        int roomCount = Random.Range(minRooms, maxRooms + 1);
        for (int i = 1; i < roomCount; i++)
        {
            currentPos = GetNextRoomPosition(currentPos);
            roomPositions.Add(currentPos);
        }

        // Step 4: Place the Shop Room at the end of the path
        currentPos += Vector2Int.right;
        roomPositions.Add(currentPos);

        // Step 5: Place the Boss Room to the right of the Shop Room
        currentPos += Vector2Int.right;
        roomPositions.Add(currentPos);
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
        possibleMoves.RemoveAll(pos => roomPositions.Contains(pos));

        if (possibleMoves.Count == 0)
        {
            return currentPos; // If no move is possible, stay in the same position
        }

        return possibleMoves.Count > 0 ? possibleMoves[Random.Range(0, possibleMoves.Count)] : currentPos;
    }

    public List<Vector2Int> GetPath() => roomPositions;
    public RoomTemplate GetStartRoom() => startRoom;
    public RoomTemplate GetShopRoom() => shopRoom;
    public RoomTemplate GetBossRoom() => bossRoom;


    /* Gizmos to be viewed in the scene for debugginig, I removed it cause it is annoying */

    /* private void OnDrawGizmos()
    {
        if (roomPositions.Count == 0)
        {
            GeneratePath();
        }

        for (int i = 0; i < roomPositions.Count; i++)
        {
            Vector3 roomPos = new Vector3(roomPositions[i].x * roomSize.x, roomPositions[i].y * roomSize.y, 0);

            if (i == 0)
                Gizmos.color = Color.green; // Start Room
            else if (i == roomPositions.Count - 2)
                Gizmos.color = Color.yellow; // Shop Room
            else if (i == roomPositions.Count - 1)
                Gizmos.color = Color.red; // Boss Room
            else
                Gizmos.color = Color.blue; // Normal Rooms

            Gizmos.DrawCube(roomPos, new Vector3(2, 2, 2));

            if (i > 0)
            {
                Vector3 prevRoomPos = new Vector3(roomPositions[i - 1].x * roomSize.x, roomPositions[i - 1].y * roomSize.y, 0);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(prevRoomPos, roomPos);
            }
        }
    } */
}
