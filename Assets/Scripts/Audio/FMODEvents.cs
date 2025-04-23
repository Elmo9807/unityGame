using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player Move SFX")]
    [field: SerializeField] public EventReference PlayerFootstepRough { get; private set; }
    [field: SerializeField] public EventReference PlayerFootstepAction { get; private set; }
    [field: SerializeField] public EventReference PlayerJump { get; private set; }
    [field: SerializeField] public EventReference PlayerDoubleJump { get; private set; }
    [field: SerializeField] public EventReference PlayerDash { get; private set; }

    [field: Header("Player Combat SFX")]
    [field: SerializeField] public EventReference BowAttack { get; private set; }
    [field: SerializeField] public EventReference SwordAttack { get; private set; }
    [field: SerializeField] public EventReference SwordHeavyAttack { get; private set; }
    [field: SerializeField] public EventReference SwordHit { get; private set; }
    [field: SerializeField] public EventReference BowHit { get; private set; }
    [field: SerializeField] public EventReference BowDeflect { get; private set; }
    [field: SerializeField] public EventReference Heal { get; private set; }
    [field: SerializeField] public EventReference PlayerHurt { get; private set; }
    [field: SerializeField] public EventReference PlayerDie { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference DungeonAmbience { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference PianoLoop { get; private set; }
    [field: SerializeField] public EventReference dungeonBgm { get; private set; }

    [field: Header("Enemy SFX")]
    [field: SerializeField] public EventReference EnemyDie { get; private set; }
    [field: SerializeField] public EventReference MageLevitate { get; private set; }
    [field: SerializeField] public EventReference MageFireballThrow { get; private set; }
    [field: SerializeField] public EventReference MageFireballExplosion { get; private set; }
    [field: SerializeField] public EventReference ArcherArrowShoot { get; private set; }
    [field: SerializeField] public EventReference ArcherFootstep { get; private set; }
    [field: SerializeField] public EventReference GruntFootstep { get; private set; }
    [field: SerializeField] public EventReference GruntAttack { get; private set; }
    [field: SerializeField] public EventReference DragonFireballThrow { get; private set; }
    [field: SerializeField] public EventReference DragonFireballExplosion { get; private set; }
    [field: SerializeField] public EventReference DragonFlying { get; private set; }
    [field: SerializeField] public EventReference DragonIdle { get; private set; }
    [field: SerializeField] public EventReference DragonBite { get; private set; }
    [field: Header("UI")]
    [field: SerializeField] public EventReference CoinPickup { get; private set; }
    [field: SerializeField] public EventReference Hover { get; private set; }
    [field: SerializeField] public EventReference Click { get; private set; }
    [field: SerializeField] public EventReference OpenMenu { get; private set; }
    [field: SerializeField] public EventReference CloseMenu { get; private set; }
    [field: SerializeField] public EventReference PlayGame { get; private set; }
    [field: SerializeField] public EventReference FailBuy { get; private set; }
    [field: SerializeField] public EventReference Buy { get; private set; }
    [field: SerializeField] public EventReference CloseShop { get; private set; }
    [field: SerializeField] public EventReference OpenShop { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            //transform.parent = null;
            //DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
            return;

        }
    }
}
