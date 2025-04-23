using UnityEngine;

public class AIChase : MonoBehaviour
{

    public GameObject player;
    public float speed;

    private float distance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not found, cannot chase.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            distance = Vector2.Distance(transform.position, player.transform.position);
            Vector2 direction = player.transform.position - transform.position;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (distance < 4)
            {
                transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(Vector3.forward * angle);
            }
        }
        else
        {
            Debug.LogWarning("Player no longer exists, cannot chase.");
        }
    }
}
