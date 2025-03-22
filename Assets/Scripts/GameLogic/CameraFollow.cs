using JetBrains.Annotations;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 2f);
    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player != null)
            {
                target = player.transform;
                Debug.Log("Camera: Player found and regrafted");
            }
            else
            {
                Debug.LogWarning("Camera: Player not found");
                return;
            }
        }

            Vector3 targetPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("Camera target set to: " + newTarget.name);

        Vector3 targetPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = targetPosition;
    }
}
