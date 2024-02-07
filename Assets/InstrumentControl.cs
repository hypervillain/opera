using UnityEngine;
using FMOD.Studio;
using System.Collections;
using System;

public class InstrumentControl : MonoBehaviour
{
    private float endOfNoteEvent;
    private Coroutine currentCoroutine;
    private CentralAudioSource instrumentAudioSource;


    public void Initialize(CentralAudioSource audioSource)
    {
        instrumentAudioSource = audioSource;
    }

    public void Play(NoteEvent note)
    {
        Debug.Log($"play note {note.index}");
        float initialVolumeControlValue = 0f;
        if (currentCoroutine != null)
        {
            Debug.Log("Stop coroutine");
            StopCoroutine(currentCoroutine);
            initialVolumeControlValue = instrumentAudioSource.GetVolumeControlValue();
        }

        float currentTime = instrumentAudioSource.ElapsedTime;
        float timeDifference = note.timing - currentTime;
        /**
            If timeDifference > 0, we're starting early. Delay sustain phase by the value of timeDifference.
            If timeDifference < 0, we're starting late. Add minimal attack to prevent clicks in the audio.
        */
        float attackDuration = timeDifference > 0 ? timeDifference : 0.08f;
        endOfNoteEvent = note.timing + note.length;

        currentCoroutine = StartCoroutine(ASRCoroutine(attackDuration, initialVolumeControlValue));
    }

    private IEnumerator ASRCoroutine(float attackDuration, float initialVolumeControlValue)
    {
        float startTime = instrumentAudioSource.ElapsedTime;
        while (instrumentAudioSource.ElapsedTime - startTime < attackDuration)
        {
            float normalizedTime = (instrumentAudioSource.ElapsedTime - startTime) / attackDuration;
            float currentValue = Mathf.Lerp(initialVolumeControlValue, 1, normalizedTime);
            instrumentAudioSource.SetVolumeControl(currentValue);
            yield return null;
        }

        instrumentAudioSource.SetVolumeControl(1f);
        while (instrumentAudioSource.ElapsedTime < endOfNoteEvent || (/** TODO: handle held notes */ false))
        {
            yield return null;
        }

        float releaseDuration = 0.15f;
        float releaseStartTime = instrumentAudioSource.ElapsedTime;
        Debug.Log($"release started");
        while (instrumentAudioSource.ElapsedTime - releaseStartTime < releaseDuration)
        {
            float normalizedTime = 1 - ((instrumentAudioSource.ElapsedTime - releaseStartTime) / releaseDuration);
            instrumentAudioSource.SetVolumeControl(normalizedTime);
            yield return null;
        }
        Debug.Log($"release over");
        instrumentAudioSource.SetVolumeControl(0f);
        currentCoroutine = null;
    }
}
