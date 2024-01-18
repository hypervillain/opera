using UnityEngine;
using FMOD.Studio;
using System.Collections;

public class InstrumentControl : MonoBehaviour
{
    private EventInstance eventInstance;

    public void Initialize(EventInstance newInstance)
    {
        eventInstance = newInstance;
    }

    public void QueueNote(float noteDuration, float timeDifference)
    {
        /**
            If timeDifference >= 0, we're starting early. Delay sustain by the value of timeDifference.
            If timeDifference < 0, we're starting late. Add minimal attack to prevent clicks in the audio.
        */
        float attackDuration;
        float sustainDuration;

        float genericAttackDuration = 0.05f;
        float genericReleaseDuration = 0.01f;

        if (timeDifference < 0)
        {
            attackDuration = genericAttackDuration;
            sustainDuration = noteDuration - genericAttackDuration - genericReleaseDuration;
        }
        else
        {
            attackDuration = timeDifference;
            sustainDuration = noteDuration - genericReleaseDuration;
        }

        Debug.Log($"ASR with {attackDuration} {sustainDuration} {genericReleaseDuration}");
        StartCoroutine(ASRCoroutine(attackDuration, sustainDuration, genericReleaseDuration));
    }

    private IEnumerator ASRCoroutine(float attackDuration, float sustainDuration, float releaseDuration)
    {
        float startTime = Time.time;
        Debug.Log("attack");
        while (Time.time - startTime < attackDuration)
        {
            float normalizedTime = (Time.time - startTime) / attackDuration;
            eventInstance.setParameterByName("VolumeControl", normalizedTime);
            yield return null;
        }

        Debug.Log("sustain");
        eventInstance.setParameterByName("VolumeControl", 1f);
        yield return new WaitForSeconds(sustainDuration);

        startTime = Time.time;
        Debug.Log("release");
        while (Time.time - startTime < releaseDuration)
        {
            float normalizedTime = 1 - ((Time.time - startTime) / releaseDuration);
            eventInstance.setParameterByName("VolumeControl", normalizedTime);
            yield return null;
        }
        Debug.Log("end");
        eventInstance.setParameterByName("VolumeControl", 0f);
    }
}
