using UnityEngine;
using FMOD.Studio;
using System.Collections;
using System;

public class InstrumentControl : MonoBehaviour
{
    private Coroutine currentCoroutine;
    private float endOfNoteEvent;
    private bool envelopeIsReleasing;
    private CentralAudioSource instrumentAudioSource;


    public void Initialize(CentralAudioSource audioSource)
    {
        instrumentAudioSource = audioSource;
    }

    public void Play(NoteEvent note)
    {
        float initialVolumeControlValue = 0f;
        if (currentCoroutine != null && envelopeIsReleasing == false)
        {
            endOfNoteEvent = note.timing + note.length;
            return;
        }
        if (currentCoroutine != null)
        {
            /**
                If envelope is releasing, use current volume value as starting value for new coroutine
            */
            initialVolumeControlValue = instrumentAudioSource.GetVolumeControlValue();
            StopCoroutine(currentCoroutine);
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

        envelopeIsReleasing = true;
        float releaseDuration = 0.150f;
        float releaseStartTime = instrumentAudioSource.ElapsedTime;
        while (instrumentAudioSource.ElapsedTime - releaseStartTime < releaseDuration)
        {
            float normalizedTime = 1 - ((instrumentAudioSource.ElapsedTime - releaseStartTime) / releaseDuration);
            instrumentAudioSource.SetVolumeControl(normalizedTime);
            yield return null;
        }
        instrumentAudioSource.SetVolumeControl(0f);
        currentCoroutine = null;
        envelopeIsReleasing = false;
    }
}
