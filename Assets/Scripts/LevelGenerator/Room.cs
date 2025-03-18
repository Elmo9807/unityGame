using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Transform> enemySpawnPoints;

    public Vector2 GetRandomSpawnPosition()
    {
        if (enemySpawnPoints.Count == 0)
        {
            return transform.position;
        }
        return enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)].position;
    }
}
