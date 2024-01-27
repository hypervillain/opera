using UnityEngine;
using FMOD.Studio;
using System.Collections;
using System;

public class InstrumentControl : MonoBehaviour
{
    private EventInstance eventInstance;
    private Coroutine currentCoroutine;
    private Func<float> getElapsedTime;
    private float endOfNoteEvent;

    private CentralAudioSource instrumentAudioSource;


    public void Initialize(CentralAudioSource audioSource)
    {
        instrumentAudioSource = audioSource;
    }

    public void Play(NoteEvent note)
    {

        if (currentCoroutine != null)
        {
            endOfNoteEvent = note.timing + note.length;
            return;
        }

        float currentTime = getElapsedTime();
        float timeDifference = note.timing - currentTime;
        /**
            If timeDifference > 0, we're starting early. Delay sustain phase by the value of timeDifference.
            If timeDifference < 0, we're starting late. Add minimal attack to prevent clicks in the audio.
        */
        float attackDuration = timeDifference > 0 ? timeDifference : 0.05f;
        endOfNoteEvent = note.timing + note.length;

        currentCoroutine = StartCoroutine(ASRCoroutine(attackDuration));
    }

    private IEnumerator ASRCoroutine(float attackDuration)
    {
        float startTime = getElapsedTime();
        while (getElapsedTime() - startTime < attackDuration)
        {
            float normalizedTime = (getElapsedTime() - startTime) / attackDuration;
            instrumentAudioSource.SetVolumeControl(normalizedTime);

            yield return null;
        }
        instrumentAudioSource.SetVolumeControl(1f);
        while (getElapsedTime() < endOfNoteEvent || (/** TODO: handle held notes */ false))
        {
            yield return null;
        }

        /** Note that extending sustain duration only works if it's done before release */
        while (getElapsedTime() - startTime < 0.5f)
        {
            float normalizedTime = 1 - ((Time.time - startTime) / 0.2f);
            instrumentAudioSource.SetVolumeControl(normalizedTime);
            yield return null;
        }
        instrumentAudioSource.SetVolumeControl(0f);
        currentCoroutine = null;
    }
}
