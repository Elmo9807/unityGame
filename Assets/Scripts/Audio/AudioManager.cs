using System.Collections;
using System.Collections.Generic;
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
        if(instance != null)
        {
            Debug.LogError("More than one audio manager in the scene is running, destroying gameObject.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ensures AudioManager persists through scenes, allowing for seemless audio playback
        }

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        masterBus = RuntimeManager.GetBus("Bus:/");
        musicBus = RuntimeManager.GetBus("Bus:/Music");
        ambienceBus = RuntimeManager.GetBus("Bus:/Ambience");
        uiSFXBus = RuntimeManager.GetBus("Bus:/UI SFX");
        gameSFXBus = RuntimeManager.GetBus("Bus:/Game SFX");
        reverbBus = RuntimeManager.GetBus("Bus:/Reverb");

    }

    private void Start() // mainly used to load bgm/ambience
    {
        InitializeAmbience(FMODEvents.instance.DungeonAmbience);
        InitializeMusic(FMODEvents.instance.dungeonBgm);
        
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
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start(); 
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

    private void CleanUp()
    {   // stops and releases created instances in scene
        foreach(EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        //  stops event emitters so that they dont stay whilst in other scenes
        }foreach(StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }
}
