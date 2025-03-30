using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player Move SFX")]
    [field: SerializeField] public EventReference PlayerFootstepRough { get; private set; }
    [field: SerializeField] public EventReference PlayerJump { get; private set; }
    [field: SerializeField] public EventReference PlayerDash { get; private set; }

    [field: Header("Player Combat SFX")]
    [field: SerializeField] public EventReference BowAttack { get; private set; }
    [field: SerializeField] public EventReference SwordAttack { get; private set; }
    [field: SerializeField] public EventReference SwordHit { get; private set; }
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference DungeonAmbience { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference PianoLoop { get; private set; }
    [field: Header("Enemy SFX")]
    [field: SerializeField] public EventReference MageLevitate { get; private set; }
    [field: SerializeField] public EventReference MageFireballThrow { get; private set; }
    [field: SerializeField] public EventReference MageFireballExplosion { get; private set; }
    [field: SerializeField] public EventReference ArcherArrowShoot { get; private set; }
    
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events script in the scene, destroying GameObject.");
            Destroy(gameObject);
        }else{
            instance = this;
            DontDestroyOnLoad(gameObject); // ensures FMODEvents persists through scenes, allowing for seemless audio playback
        }
        
    }
}
