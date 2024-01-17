using UnityEngine;
using FMOD.Studio;
using FMODUnity;

using System.Collections;

public class InstrumentManager : MonoBehaviour
{
    private EventInstance noteEvent;

    private string[] fmodEventNames;
    private FMOD.Studio.EventInstance[] eventInstances;

    private bool isButtonDownAcrossUpdates = false;
    void Start()
    {
        fmodEventNames = new string[] { "event:/MyInstrument0", "event:/MyInstrument1" };
        eventInstances = new EventInstance[fmodEventNames.Length];

        for (int i = 0; i < fmodEventNames.Length; i++)
        {
            eventInstances[i] = RuntimeManager.CreateInstance(fmodEventNames[i]);
        }
    }

    public void PlayNote(int noteIndex)
    {
        if (noteIndex > 0)
        {
            // release previous note, if any
            eventInstances[noteIndex - 1].setPaused(true);
        }
        eventInstances[noteIndex].setPaused(false);
        eventInstances[noteIndex].start();

        // StartCoroutine(PauseAfterDelay(noteIndex, (float)0.8));
    }

    private IEnumerator PauseAfterDelay(int noteIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventInstances[noteIndex].setPaused(true);
    }

    private void HandleClickUpdate()
    {
        bool isButtonDown = Input.GetMouseButton(0);
        if (isButtonDown && !isButtonDownAcrossUpdates)
        {
            Debug.Log("Button was just pressed");
        }
        if (isButtonDown && isButtonDownAcrossUpdates)
        {
            Debug.Log("Button was already pressed / checked!");
        }
        else if (!isButtonDown && isButtonDownAcrossUpdates)
        {
            Debug.Log("The Button was just released");
        }
        isButtonDownAcrossUpdates = isButtonDown;
    }

    void Update()
    {
        HandleClickUpdate();
    }
}
