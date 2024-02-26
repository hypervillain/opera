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
    public bool isDebug;
    public UnityEngine.UI.Text debugText;

    private Coroutine subdivisionCoroutine;

    private int SUBDIVISIONS = 8;

    private float beatDuration = 60f / 115; // TODO: query BPM;

    private int signature = 4; // TODO: query signature;

    private List<NoteEvent> noteEvents;

    public CentralAudioSource centralAudioSource;


    private bool isButtonDownAcrossUpdates = false;
    private InstrumentControl instrumentControl;
    private ScoreManager scoreManager;
    private BeatTracker beatTracker;

    public SceneData sceneData; // meh
    public SceneData[] scenes;
    public static ActManager Instance { get; private set; }

    public static event Action<float> OnElapsedTimeChanged;
    public static event Action<List<NoteEvent>> OnMelodyReady;
    public static event Action<int, int> OnBPMReady;
    public static event Action<BeatTracker> OnBeatTrackerUpdate;

    // private IEnumerator SubdivisionRoutine(int measure, int beat)
    // {
    //     float subdivisionDuration = beatDuration / SUBDIVISIONS;
    //     for (int i = 1; i <= SUBDIVISIONS; i++)
    //     {
    //         yield return new WaitForSeconds(subdivisionDuration);
    //     }
    // }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Should I?
    }
    void Start()
    {
        instrumentControl = gameObject.AddComponent<InstrumentControl>();
        scoreManager = gameObject.AddComponent<ScoreManager>();
        if (centralAudioSource == null)
        {
            throw new Exception("Please add centralAudioSource to ActManager");
        }

        CentralAudioSource.OnAudioBeat += OnAudioBeat;

        LoadScene(0);
    }

    private void LoadScene(int index)
    {
        sceneData = scenes[index];
        noteEvents = scoreManager.Initialize(sceneData.songDataName);
        OnMelodyReady(noteEvents);

        beatTracker = new BeatTracker(sceneData.bpm, sceneData.signature);
        OnBPMReady(sceneData.bpm, sceneData.signature);

        centralAudioSource.Play(sceneData.FMODEventName, sceneData.bpm, sceneData.signature);
        instrumentControl.Initialize(centralAudioSource);
    }

    private void OnAudioBeat(int FMODMeasure, int FMODBeat)
    {
        beatTracker.OnBeatUpdate(FMODMeasure, FMODBeat, centralAudioSource.ElapsedTime);
        OnBeatTrackerUpdate(beatTracker);
        // if (subdivisionCoroutine != null)
        // {
        //     StopCoroutine(subdivisionCoroutine);
        // }
        // subdivisionCoroutine = StartCoroutine(SubdivisionRoutine(FMODMeasure, FMODBeat));
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
        // if (isDebug)
        // {
        //     LogDebugInfo(centralAudioSource.ElapsedTime);
        // }
        OnElapsedTimeChanged(centralAudioSource.ElapsedTime); // no
        // UpdateNoteStatus();
        // HandleClickUpdate();
    }

    void LogDebugInfo(float time)
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
