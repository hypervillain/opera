using System;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

// Marshall
using System.Runtime.InteropServices;


public class CentralAudioSource : MonoBehaviour
{
    private EventInstance eventInstance;
    private StudioEventEmitter eventEmitter;

    public static event Action<int, int> OnAudioBeat;

    void Start()
    {
        eventEmitter = GetComponent<StudioEventEmitter>();

        if (eventEmitter)
        {
            eventInstance = eventEmitter.EventInstance;
            eventInstance.setCallback(TimelineCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
        }
        else
        {
            Debug.LogError("StudioEventEmitter component not found on object CentralAudioSource.");
        }
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static private FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var beatParams = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
            int currentBeat = beatParams.beat;
            int currentMeasure = beatParams.bar;

            OnAudioBeat?.Invoke(currentBeat, currentMeasure);
        }

        return FMOD.RESULT.OK;
    }

    void OnDestroy()
    {
        // No need to destroy the emitter I guess?
    }
}
