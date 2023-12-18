using System;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

// Marshall
using System.Runtime.InteropServices;


public class ActManager : MonoBehaviour
{
    public Metronome metronome;
    public FMODUnity.StudioEventEmitter[] studioEventEmittersToPause;

    private bool PLAY_TOGGLE = true;

    private FMOD.Studio.EventInstance pauseSnapshotInstance;


    void Start()
    {
        CentralAudioSource.OnAudioBeat += OnAudioBeat;
        Metronome.OnCubeClicked += OnMetronomeClick;

        pauseSnapshotInstance = FMODUnity.RuntimeManager.CreateInstance("snapshot:/GamePause");
    }

    void OnDisable()
    {
        CentralAudioSource.OnAudioBeat -= OnAudioBeat;
        Metronome.OnCubeClicked -= OnMetronomeClick;
    }

    private void OnMetronomeClick()
    {
        Debug.Log("Click!");
        PLAY_TOGGLE = !PLAY_TOGGLE;
        if (PLAY_TOGGLE == false)
        {
            pauseSnapshotInstance.start();
            foreach (var emitter in studioEventEmittersToPause)
            {
                var instance = emitter.EventInstance;
                instance.setPaused(true);
            }
        }
        else
        {
            pauseSnapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            foreach (var emitter in studioEventEmittersToPause)
            {
                var instance = emitter.EventInstance;
                instance.setPaused(false);
            }
        }
    }

    private void OnAudioBeat(int beat, int measure)
    {
        if (metronome != null)
        {
            metronome.Rotate();
        }
    }
}