using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player Move SFX")]
    [field: SerializeField] public EventReference PlayerFootstepRough { get; private set; }
    [field: SerializeField] public EventReference PlayerJump { get; private set; }
    
    [field: Header("Player Combat SFX")]
    [field: SerializeField] public EventReference BowAttack { get; private set; }
    [field: SerializeField] public EventReference SwordAttack { get; private set; }
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference DungeonAmbience { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference PianoLoop { get; private set; }
    [field: Header("Enemy SFX")]
    [field: SerializeField] public EventReference MageLevitate { get; private set; }
    
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            Debug.LogError("Found more than one FMOD Events script in the scene.");
        }
        instance = this;
    }
}
