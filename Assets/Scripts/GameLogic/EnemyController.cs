using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Enemy EnemyData { get; private set; }
    private void Start()
    {
        EnemyData = null;
        EnemyData.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        Destroy(gameObject);
    }
}
