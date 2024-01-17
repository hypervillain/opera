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

    public InstrumentManager instrumentManager;

    private bool PLAY_TOGGLE = true;

    private EventInstance pauseSnapshotInstance;

    private Coroutine subdivisionCoroutine;

    private int lastMeasurePlayed = 0;
    private int measure;

    private int SUBDIVISIONS = 8;

    private float beatDuration = 60f / 115; // TODO: Infer BPM;

    private bool isButtonDownAcrossUpdates = false;

    private IEnumerator SubdivisionRoutine(int measure, int beat)
    {
        float subdivisionDuration = beatDuration / SUBDIVISIONS;
        for (int i = 0; i < SUBDIVISIONS; i++)
        {
            if (i == 0)
            {
                metronome.Rotate();
                // instrumentManager.PlayNote(0);
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

    void Update()
    {
        HandleClickUpdate();
    }

    private void TogglePlayAct()
    {
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
        if (lastMeasurePlayed < measure)
        {

        }
        if (metronome != null)
        {
            metronome.Rotate();
        }

        if (subdivisionCoroutine != null)
        {
            StopCoroutine(subdivisionCoroutine);
        }

        subdivisionCoroutine = StartCoroutine(SubdivisionRoutine(FMODMeasure, FMODBeat));
        lastMeasurePlayed = measure;
    }

    private IEnumerator ADSRCoroutine(int direction)
    {
        float attackTime = 0.1f;
        float startTime = Time.time;
        while (Time.time - startTime < attackTime)
        {
            float normalizedTime;
            if (direction > 0)
            {
                normalizedTime = (Time.time - startTime) / attackTime;
            }
            else
            {
                normalizedTime = 1 - ((Time.time - startTime) / attackTime);
            }
            studioEventEmittersToPause[0].EventInstance.setParameterByName("VolumeControl", normalizedTime);
            yield return null;
        }
    }

    public void VolumeUp()
    {
        StartCoroutine(ADSRCoroutine(1));
    }

    public void VolumeDown()
    {
        StartCoroutine(ADSRCoroutine(0));
    }

    private void HandleClickUpdate()
    {
        bool isButtonDown = Input.GetMouseButton(0);
        if (isButtonDown && !isButtonDownAcrossUpdates)
        {
            VolumeUp();
        }
        if (isButtonDown && isButtonDownAcrossUpdates)
        {
            // Debug.Log("Button was already pressed / checked!");
        }
        else if (!isButtonDown && isButtonDownAcrossUpdates)
        {
            VolumeDown();
        }
        isButtonDownAcrossUpdates = isButtonDown;
    }
}