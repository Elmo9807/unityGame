using UnityEngine;

public class MusicChangeTrigger : MonoBehaviour
{

    [Header("Area")]
    [SerializeField] private MusicArea area;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player")) 
        { 
        
            AudioManager.instance.SetMusicArea(area);
        }
    }
}
