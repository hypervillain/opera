using UnityEngine;
using FMOD.Studio;
using FMODUnity;

using System.Collections;

using System;

using System.Linq;

public class Note
{
    public string value;
    public int measure;
    public int beat;
    public int subdivision;

    public float expectedAbsoluteTime;

    public bool isPast = false;
    public bool isHit = false;

    public Note(string value, int measure, int beat, int subdivision)
    {
        this.value = value;
        this.measure = measure;
        this.beat = beat;
        this.subdivision = subdivision;
    }

    public int CountSubdivisions(int SIGNATURE, int SUBDIVISIONS, int measure, int beat, int subdivision)
    {
        return (measure - 1) * SIGNATURE * SUBDIVISIONS + (beat - 1) * SUBDIVISIONS + (subdivision - 1);
    }

    public float UpdateExpectedAbsoluteTime(int SIGNATURE, float BEAT_DURATION, int SUBDIVISIONS, float currentTime, int measure, int beat, int subdivision)
    {
        int fromTotalSubdivisions = CountSubdivisions(SIGNATURE, SUBDIVISIONS, measure, beat, subdivision);
        int thisTotalSubdivisions = CountSubdivisions(SIGNATURE, SUBDIVISIONS, this.measure, this.beat, this.subdivision);
        int offsetSubdivisions = thisTotalSubdivisions - fromTotalSubdivisions;
        float offset = offsetSubdivisions * (BEAT_DURATION / SUBDIVISIONS);
        this.expectedAbsoluteTime = currentTime + offset;
        // Debug.Log($"Note: {this.value}, Offset subdivisions: {offsetSubdivisions}, Time offset: {timeOffset}");

        return this.expectedAbsoluteTime;
    }
}

public class ActManager : MonoBehaviour
{
    public Restart restart;
    public StudioEventEmitter[] studioEventEmittersToPause;

    private bool PLAY_TOGGLE = true;

    private EventInstance pauseSnapshotInstance;

    private Coroutine subdivisionCoroutine;

    private int SUBDIVISIONS = 8;

    private float beatDuration = 60f / 115; // TODO: query BPM;
    private int signature = 4; // TODO: query signature;

    private bool isButtonDownAcrossUpdates = false;

    private Note[] notes;

    private (int measure, int beat, int subdivision, float time) currentBeatInfo;


    public void OnPlayPause()
    {
        TogglePlayAct();
    }

    /** END OF PUBLIC METHODS */
    private void CalculateNoteTimeOffsets(int measure, int beat)
    {
        float currentTime = Time.time;
        foreach (Note note in notes)
        {
            note.UpdateExpectedAbsoluteTime(signature, beatDuration, SUBDIVISIONS, currentTime, measure, beat, 1);
        }
    }

    private IEnumerator SubdivisionRoutine(int measure, int beat)
    {
        float subdivisionDuration = beatDuration / SUBDIVISIONS;
        for (int i = 1; i <= SUBDIVISIONS; i++)
        {
            currentBeatInfo = (measure, beat, i, Time.time);
            yield return new WaitForSeconds(subdivisionDuration);
        }
    }

    void Start()
    {
        notes = new Note[9];
        notes[0] = new Note("A", 1, 1, 1);
        notes[1] = new Note("A#", 1, 2, 1);
        notes[2] = new Note("B", 1, 3, 5);
        notes[3] = new Note("B#", 1, 3, 5);
        notes[4] = new Note("C", 1, 3, 5);
        notes[5] = new Note("C#", 2, 1, 5);
        notes[6] = new Note("D", 2, 3, 1);
        notes[7] = new Note("D#", 2, 3, 5);
        notes[8] = new Note("E", 2, 4, 1);

        CalculateNoteTimeOffsets(1, 1);

        CentralAudioSource.OnAudioBeat += OnAudioBeat;
        Restart.OnRestartClicked += OnRestartClick;

        pauseSnapshotInstance = RuntimeManager.CreateInstance("snapshot:/GamePause");
    }

    void OnDisable()
    {
        CentralAudioSource.OnAudioBeat -= OnAudioBeat;
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
        foreach (var emitter in studioEventEmittersToPause)
        {
            var instance = emitter.EventInstance;
            instance.setTimelinePosition(0);
        }

    }

    private void OnAudioBeat(int FMODBeat, int FMODMeasure)
    {
        if (subdivisionCoroutine != null)
        {
            StopCoroutine(subdivisionCoroutine);
        }

        CalculateNoteTimeOffsets(FMODMeasure, FMODBeat);
        subdivisionCoroutine = StartCoroutine(SubdivisionRoutine(FMODMeasure, FMODBeat));
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

    private (Note closestNote, float timeDistance) FindClosestNote()
    {
        Note closestNote = null;
        float currentTime = Time.time;
        float smallestTimeDifference = float.MaxValue;

        foreach (Note note in notes)
        {
            if (!note.isPast)
            {
                float timeDifference = Mathf.Abs(note.expectedAbsoluteTime - currentTime);
                if (timeDifference < smallestTimeDifference)
                {
                    smallestTimeDifference = timeDifference;
                    closestNote = note;
                }
            }
        }
        return (closestNote, smallestTimeDifference);
    }
    private void HandleClickUpdate()
    {
        bool isButtonDown = Input.GetMouseButton(0);
        if (isButtonDown && !isButtonDownAcrossUpdates)
        {
            (Note closestNote, float timeDistance) = FindClosestNote();
            if (closestNote != null)
            {
                Debug.Log($"Closest note is {closestNote.value}, with a difference of {timeDistance}");
                /** Check if it's actually in range! */
                closestNote.isPast = true;
                closestNote.isHit = true;

                /** Handle time difference logic here! */
                StartCoroutine(ADSRCoroutine(1));
            }
        }
        else if (!isButtonDown && isButtonDownAcrossUpdates)
        {
            StartCoroutine(ADSRCoroutine(0));
        }
        isButtonDownAcrossUpdates = isButtonDown;
    }

    private void UpdateNoteStatus()
    {
        float currentTime = currentBeatInfo.time;
        float toleranceWindow = 0.150f; // Test this!

        Note note = notes.FirstOrDefault(note => !note.isPast);
        if (note == null)
        {
            int notesSuccess = notes.Count(note => note.isHit);
            Debug.Log($"You clicked successfully {notesSuccess} notes!");
        }
        if (note != null && !note.isPast && currentTime > note.expectedAbsoluteTime + toleranceWindow)
        {
            note.isPast = true;
            Debug.Log($"Missed note {note.value}!");
        }
    }

    void Update()
    {
        UpdateNoteStatus();
        HandleClickUpdate();
    }
}
