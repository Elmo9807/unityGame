using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    public float masterVolume = 1;
    [Range(0,1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume = 1;
    [Range(0, 1)]
    public float uiSFXVolume = 1;
    [Range(0, 1)]
    public float gameSFXVolume = 1;
    [Range(0, 1)]

    private Bus masterBus, musicBus, ambienceBus, uiSFXBus, gameSFXBus, reverbBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;

    public static AudioManager instance { get; private set; } // Singleton: other scripts can read this instance (public), but only AudioManager can modify instance property

    private void Awake()

    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>(); // for objects using persisting spatial audio

        masterBus = RuntimeManager.GetBus("Bus:/");
        musicBus = RuntimeManager.GetBus("Bus:/Music");
        ambienceBus = RuntimeManager.GetBus("Bus:/Ambience");
        uiSFXBus = RuntimeManager.GetBus("Bus:/UI SFX");
        gameSFXBus = RuntimeManager.GetBus("Bus:/Game SFX");
        reverbBus = RuntimeManager.GetBus("Bus:/Reverb");

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Start()
    {
        
    }

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        ambienceBus.setVolume(ambienceVolume);
        uiSFXBus.setVolume(uiSFXVolume);
        gameSFXBus.setVolume(gameSFXVolume); reverbBus.setVolume(gameSFXVolume); // game SFX is routed to the reverb in FMOD, so their volumes must change at the same time

    }

    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateInstance(ambienceEventReference);
        ambienceEventInstance.setParameterByName("ambience_intensity", 1f);
        ambienceEventInstance.start();
        eventInstances.Add(ambienceEventInstance);
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
        eventInstances.Add(musicEventInstance);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("AudioManager: Changing scene");
        CleanUp();
        if (scene.name == "Dungeon01")
        {
            InitializeAmbience(FMODEvents.instance.DungeonAmbience);
            InitializeMusic(FMODEvents.instance.dungeonBgm);
        }
        
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos) // Used for playing short SFXs like jumping, picking up items, UI, etc.
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    public void pauseMute() // mutes all buses except ui
    {
        musicBus.setMute(true);
        ambienceBus.setMute(true);
        gameSFXBus.setMute(true);
    }

    public void pauseUnmute()
    {
        musicBus.setMute(false);
        ambienceBus.setMute(false);
        gameSFXBus.setMute(false);
    }

    public void FadeoutAll()
    {
        if (eventInstances != null)
        {
            foreach (EventInstance eventInstance in eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                eventInstance.release();
            }
        }
    }

    public void SetMusicArea(MusicArea area)
    {
        musicEventInstance.setParameterByName("area", (float) area);
        Debug.Log($"Setting music enum to {area}");
    }

    private void CleanUp()
    {
        // stops and releases created eventinstances in scene, e.g. footsteps, bgm
        if (eventInstances != null)
        {
            foreach (EventInstance eventInstance in eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
            eventInstances.Clear();
        }
        

        // stops event emitters so that they don't stay while in other scenes
        if(eventEmitters != null)
        {
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                if (emitter.IsPlaying()) emitter.Stop();
            }
            eventEmitters.Clear();
        }
        
    }

    private void OnDestroy()
    {
        CleanUp();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
