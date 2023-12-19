using UnityEngine;
using FMOD.Studio;
using FMODUnity;

using System.Collections;

// Marshall
using System.Runtime.InteropServices;


public class ActManager : MonoBehaviour
{
    public Metronome metronome;
    public Restart restart;
    public StudioEventEmitter[] studioEventEmittersToPause;

    private bool PLAY_TOGGLE = true;

    private EventInstance pauseSnapshotInstance;

    private Coroutine subdivisionCoroutine;

    private int lastMeasurePlayed = 0;
    private int measure;

    private IEnumerator SubdivisionRoutine(int subdivisions, float beatDuration, bool fullRotate)
    {
        float subdivisionDuration = beatDuration / subdivisions;
        for (int i = 0; i < subdivisions; i++)
        {
            if (fullRotate || i == 4)
            {
                metronome.Rotate();
            }
            yield return new WaitForSeconds(subdivisionDuration);
        }
    }


    void Start()
    {
        CentralAudioSource.OnAudioBeat += OnAudioBeat;
        Metronome.OnCubeClicked += OnMetronomeClick;
        Restart.OnRestartClicked += OnRestartClick;

        pauseSnapshotInstance = RuntimeManager.CreateInstance("snapshot:/GamePause");
    }

    void OnDisable()
    {
        CentralAudioSource.OnAudioBeat -= OnAudioBeat;
        Metronome.OnCubeClicked -= OnMetronomeClick;
    }

    private void TogglePlayAct()
    {
        // Toggle play status
        PLAY_TOGGLE = !PLAY_TOGGLE;
        if (PLAY_TOGGLE == false)
        {
            // Trigger FMOD GamePause Snapshot
            pauseSnapshotInstance.start();
            Debug.Log(studioEventEmittersToPause.Length);
            foreach (var emitter in studioEventEmittersToPause)
            {
                var instance = emitter.EventInstance;
                // Pause all instances specified in component.
                // This usually includes everything but Ambiance
                instance.setPaused(true);
            }
        }
        else
        {
            // Stop FMOD GamePause Snapshot
            pauseSnapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            foreach (var emitter in studioEventEmittersToPause)
            {
                var instance = emitter.EventInstance;
                // Play all instances
                instance.setPaused(false);
            }
        }
    }

    private void OnRestartClick()
    {
        lastMeasurePlayed = 0;
        foreach (var emitter in studioEventEmittersToPause)
        {
            var instance = emitter.EventInstance;
            instance.setTimelinePosition(0);
        }

    }

    private void OnMetronomeClick()
    {
        TogglePlayAct();
    }

    private void OnAudioBeat(int FMODBeat, int FMODMeasure)
    {
        measure = FMODMeasure;
        if (metronome != null)
        {
            metronome.Rotate();
        }

        if (subdivisionCoroutine != null)
        {
            StopCoroutine(subdivisionCoroutine);
        }

        // TODO: Infer BPM from CentralAudioSource instance;
        float beatDuration = 60f / 115;
        int subdivisions = 8;
        subdivisionCoroutine = StartCoroutine(SubdivisionRoutine(subdivisions, beatDuration, lastMeasurePlayed < measure));
        lastMeasurePlayed = measure;
    }
}
