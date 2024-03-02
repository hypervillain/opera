using System;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Runtime.InteropServices;

public class CentralAudioSource : MonoBehaviour
{
    private static CentralAudioSource instance;
    private EventInstance eventInstance;
    private bool isEventStarted = false;
    private bool isEventPlaying = false;

    private int bpm;
    private int signature;

    private float elapsedTime = 0;
    public static event Action<int, int> OnAudioBeat;
    public bool IsPlaying => isEventPlaying;
    public float ElapsedTime => elapsedTime;

    public EventInstance Play(string eventName, int _bpm, int _signature)
    {
        bpm = _bpm;
        instance = this;
        signature = _signature;
        if (isEventStarted)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        eventInstance = RuntimeManager.CreateInstance(eventName);
        eventInstance.setCallback(TimelineCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
        eventInstance.start();
        isEventStarted = true;
        isEventPlaying = true;
        return eventInstance;
    }

    private void CalculateElapsedTime(int measure, int beat)
    {
        int totalBeats = (measure - 1) * signature + (beat - 1);
        elapsedTime = totalBeats * 60f / bpm;
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static private FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var beatParams = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            int beat = beatParams.beat;
            int measure = beatParams.bar;

            instance?.CalculateElapsedTime(measure, beat);

            OnAudioBeat?.Invoke(measure, beat);
        }

        return FMOD.RESULT.OK;
    }

    public void SetVolumeControl(float value)
    {
        if (isEventStarted)
        {
            eventInstance.setParameterByName("VolumeControl", value);
        }
    }

    public float GetVolumeControlValue()
    {
        if (isEventStarted)
        {
            eventInstance.getParameterByName("VolumeControl", out float parameterValue);
            return parameterValue;
        }
        return 0;
    }

    public void TogglePauseEvent()
    {
        if (isEventStarted)
        {
            if (isEventPlaying)
            {
                Debug.Log("Pausing FMOD event");
                eventInstance.setPaused(true);
            }
            else
            {
                eventInstance.setPaused(false);
            }
            isEventPlaying = !isEventPlaying;
        }
    }

    public void StopEvent()
    {
        if (isEventStarted)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
            isEventStarted = false;
        }
    }

    void Update()
    {
        if (isEventStarted && isEventPlaying)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        if (isEventStarted)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }
}