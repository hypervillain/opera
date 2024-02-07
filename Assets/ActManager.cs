using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using FMOD;

public class ActManager : MonoBehaviour
{
    public bool debug;
    public UnityEngine.UI.Text debugText;

    private Coroutine subdivisionCoroutine;

    private int SUBDIVISIONS = 8;

    private float beatDuration = 60f / 115; // TODO: query BPM;

    private int signature = 4; // TODO: query signature;

    private bool isButtonDownAcrossUpdates = false;

    private List<NoteEvent> noteEvents;

    private InstrumentControl instrumentControl;
    private ScoreManager scoreManager;
    public CentralAudioSource centralAudioSource;

    private (int measure, int beat, int subdivision, float time) currentBeatInfo;

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
        instrumentControl = gameObject.AddComponent<InstrumentControl>();
        scoreManager = gameObject.AddComponent<ScoreManager>();

        noteEvents = scoreManager.Initialize("Song1");

        if (centralAudioSource == null)
        {
            throw new Exception("Please add centralAudioSource to ActManager");
        }
        centralAudioSource.Play("event:/Act1", 115, 4);
        instrumentControl.Initialize(centralAudioSource);

        CentralAudioSource.OnAudioBeat += OnAudioBeat;
    }

    private void OnAudioBeat(int FMODMeasure, int FMODBeat)
    {
        if (subdivisionCoroutine != null)
        {
            StopCoroutine(subdivisionCoroutine);
        }
        subdivisionCoroutine = StartCoroutine(SubdivisionRoutine(FMODMeasure, FMODBeat));
    }

    private void HandleClickUpdate()
    {
        bool isButtonDown = Input.GetMouseButton(0);
        if (isButtonDown && !isButtonDownAcrossUpdates)
        {
            NoteEvent closestNote = scoreManager.GetNextNote();
            if (closestNote != null)
            {
                float toleranceWindow = 0.150f;
                float timeDifference = closestNote.timing - centralAudioSource.ElapsedTime;

                /**
                    Use these to test instrument!
                */
                bool isVeryWellInTime = Math.Abs(timeDifference) < 0.1;
                bool IsSlightlyOffBeatAfter = closestNote.timing < centralAudioSource.ElapsedTime && Math.Abs(timeDifference) > 0.1 && Math.Abs(timeDifference) < toleranceWindow;
                bool IsSlightlyOffBeatBefore = closestNote.timing > centralAudioSource.ElapsedTime && Math.Abs(timeDifference) > 0.1 && Math.Abs(timeDifference) < toleranceWindow;
                if (isVeryWellInTime && UnityEngine.Random.value < 0.5f)
                {

                    instrumentControl.Play(closestNote);

                    closestNote.isPast = true;
                    closestNote.isHit = true;
                }
            }
        }
        else if (!isButtonDown && isButtonDownAcrossUpdates)
        {

        }
        isButtonDownAcrossUpdates = isButtonDown;
    }

    private void UpdateNoteStatus()
    {
        float toleranceWindow = 0.150f; // Test this!

        NoteEvent note = noteEvents.FirstOrDefault(note => !note.isPast);
        if (note == null)
        {
            int notesSuccess = noteEvents.Count(note => note.isHit);
        }
        if (note != null)
        {
            float timeDifference = note.timing - centralAudioSource.ElapsedTime;
            bool isVeryWellInTime = Math.Abs(timeDifference) < 0.1;
            if (isVeryWellInTime && UnityEngine.Random.value < 0.01f)
            {

                instrumentControl.Play(note);

                note.isPast = true;
                note.isHit = true;
            }
        }
        // if (note != null && centralAudioSource.ElapsedTime > note.timing + toleranceWindow)
        // {
        //     note.isPast = true;
        //     // Debug.Log($"Missed note {note.value}!");
        // }
    }

    void Update()
    {
        if (debug)
        {
            Debug(centralAudioSource.ElapsedTime);
        }
        UpdateNoteStatus();
        HandleClickUpdate();
    }

    void Debug(float time)
    {
        if (debugText != null)
        {
            debugText.text = time.ToString();
        }
    }

    void OnDisable()
    {
        CentralAudioSource.OnAudioBeat -= OnAudioBeat;
    }
}
