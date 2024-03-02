using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool shouldPlayEffect;

    public CentralAudioSource centralAudioSource; // no

    public static event Action<RopeHelpers.RopeDirection> OnRopeDirectionUpdate;

     void Awake()
    {
        ActManager.OnBeatTrackerUpdate += OnBeatTrackerUpdate;
        ActManager.OnElapsedTimeChanged += OnElapsedTimeChanged;
    }

    void Start()
    {
        
    }

    public void SetShouldPlay()
    {
        shouldPlayEffect = true;
    }

    void PlayEffect(BeatTracker beatTracker)
    {

        FMODUnity.RuntimeManager.PlayOneShot("event:/RopeDirectionSWitch");
        ActManager.Instance.CurrentSceneData.ropeDirection = RopeHelpers.SwitchRopeDirection(ActManager.Instance.CurrentSceneData.ropeDirection);
        // float pauseDuration = BeatHelpers.GetSecondsPerBeatFromBPM(beatTracker.bpm) * 1;
        // centralAudioSource.TogglePauseEvent();
        // melodySpawner.SwitchRopeDirection(); // this is supposed to replace melody event positions on the rope
        // CoroutineExec.ExecuteAfterDelay(
        //     this,
        //     pauseDuration,
        //     () => centralAudioSource.TogglePauseEvent()
        // );
    }

    void OnBeatTrackerUpdate(BeatTracker beatTracker) {
        if (shouldPlayEffect)
        {
            PlayEffect(beatTracker);
            shouldPlayEffect = false;
        }
        // if (beatTracker.measure != 1 && beatTracker.beat == 1)
        // {
        //     PlayEffect(beatTracker);
        // }
    }

    void OnElapsedTimeChanged(float elapsedTime)
    {
        // compare with statistically generated random value to possibly launch effect
        // This is where level difficulty might come into play
    }
}