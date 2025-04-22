using UnityEngine;

public class FMODListener : MonoBehaviour
{
    // As the sound effects occur at z = 0, but the camera is positioned at z = -10, we set a GameObject that follows the camera, but offseted to z = 0.
    // That way, the audio's spatiality will be more accurate.
    private void LateUpdate() // ensures all updates are done before moving gameobject to camera, to prevent weird desync
    {
        Vector3 camPos = Camera.main.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, 0f); // force z = 0
    }
}