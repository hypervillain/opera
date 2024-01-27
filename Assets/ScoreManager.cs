using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

using System.Linq;

public class ScoreManager : MonoBehaviour
{
    private List<NoteEvent> noteEvents;
    public List<NoteEvent> Initialize(string songName)
    {
        noteEvents = new();
        string filePath = Application.dataPath + "/Songs/" + songName + ".csv";

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file at {filePath} could not be found.");
        }

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] values = line.Split(',');
            if (values.Length == 3)
            {
                float time;
                if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out time))
                {
                    bool fill = false;
                    if (values[2] == "1")
                    {
                        fill = true;
                    }
                    NoteEvent data = new() { index = i, value = values[1], timing = time, fill = fill };
                    noteEvents.Add(data);
                }
            }
        }
        CalculateNoteLengths();
        return noteEvents;
    }

    public NoteEvent GetNextNote(float time)
    {
        return noteEvents.FirstOrDefault(noteEvent => !noteEvent.isPast);
    }

    private void UpdateNoteStatus(float elapsedTime)
    {
        float toleranceWindow = 0.150f;

        NoteEvent note = GetNextNote(elapsedTime);
        if (note == null)
        {
            Debug.Log("Done!");
        }
        if (note != null && elapsedTime > note.timing + toleranceWindow)
        {
            note.isPast = true;
        }
    }

    /**
        Calculated here out of convenience.
        A note length is its timing difference with next note.
        If it's "filled", its length is calculated until next note has no true fill member.
    */
    private void CalculateNoteLengths()
    {
        for (int i = 0; i < noteEvents.Count; i++)
        {
            if (noteEvents[i].fill)
            {
                int j = i + 1;
                while (j < noteEvents.Count && noteEvents[j].fill)
                {
                    j++;
                }

                noteEvents[i].length = (j < noteEvents.Count) ? noteEvents[j].timing - noteEvents[i].timing : float.MaxValue;
            }
            else if (i < noteEvents.Count - 1)
            {
                noteEvents[i].length = noteEvents[i + 1].timing - noteEvents[i].timing;
            }
            else
            {
                noteEvents[i].length = float.MaxValue;
            }
        }
    }
}



public class NoteEvent
{
    public int index;
    public string value;
    public float timing;
    public float length;
    public bool fill;

    public bool isPast;

    // Temporary
    public bool isHit;
}
