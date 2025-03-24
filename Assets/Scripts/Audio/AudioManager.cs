using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; } // Singleton: other scripts can read this instance (public), but only AudioManager can modify instance property

    private void Awake()

    {
        if(instance == null)
        {
            Debug.LogError("More than one audio manager in the scene is running!");
        }
        instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos) // Used for playing short SFXs like jumping, picking up items, UI, etc.
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }
}
