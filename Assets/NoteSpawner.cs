using System.Collections;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform partition;
    public float minSpawnDelay = 0.02f;
    public float maxSpawnDelay = 0.05f;
    public float noteSpeed = 5.0f;

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnNote();
            nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
        }
    }

    void SpawnNote()
    {
        // Choose a random lane from 1 to 4 for spawning
        int noteLane = Random.Range(1, 5); // This will give us lanes 1, 2, 3, or 4 for spawning
        float segmentWidth = partition.localScale.x / (4 + 1) * 10; // Divide plane's width into 5 segments for 4 lanes
        // The x position is determined by the left edge plus half a section width, plus the full width of each lane up to the chosen lane
        float xPosition = (partition.localScale.x * 5) + (-segmentWidth * noteLane);

        // Adjust the Y-position for the height of the plane at the tilt
        float spawnHeight = partition.localScale.z / 2 * Mathf.Cos(Mathf.Deg2Rad * partition.eulerAngles.x);

        // Calculate the spawn position at the top of the partition
        Vector3 spawnPosLocal = new Vector3(xPosition, spawnHeight, partition.localScale.z * 2);
        Vector3 spawnPosWorld = partition.TransformPoint(spawnPosLocal);

        // Instantiate the note at the spawn position
        GameObject note = Instantiate(notePrefab, spawnPosWorld, partition.rotation);
        note.transform.SetParent(partition); // Make the note a child of the partition to follow its rotation

        // Assign the color based on the lane
        Material noteMaterial = note.GetComponent<Renderer>().material;
        noteMaterial.color = GetColorForLane(noteLane);

        Debug.Log("Color: " + noteMaterial.color + "Position: " + xPosition);

        // Start the coroutine to move the note down the plane
        StartCoroutine(MoveNoteDownThePlane(note.transform));
    }


    IEnumerator MoveNoteDownThePlane(Transform noteTransform)
    {
        // Calculate the local Z endpoint considering the tilt
        float despawnZ = -partition.localScale.z * 3;

        while (noteTransform.localPosition.z > despawnZ)
        {
            // Move the note in local space along the plane's local Z-axis (forward direction)
            noteTransform.Translate(0, 0, -noteSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
        Destroy(noteTransform.gameObject); // Destroy the note when it reaches the end of the slope
    }



    Color GetColorForLane(int lane)
    {
        switch (lane)
        {
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.red;
            case 4: return new Color(1, 0.64f, 0); // Orange
            default: return Color.white;
        }
    }
}
