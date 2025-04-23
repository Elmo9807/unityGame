using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public Transform enemyParent;
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;

    public void SpawnEnemiesInRoom(GameObject room)
    {
        Room roomComponent = room.GetComponent<Room>();
        if (roomComponent == null)
        {
            return;
        }

        int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom);
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPosition = roomComponent.GetRandomSpawnPosition();
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
        }
    }

}
