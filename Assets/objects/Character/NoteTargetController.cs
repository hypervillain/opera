using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class NoteTargetController : MonoBehaviour
{
    public int dev_elementIndex = 10;
    public GameObject player;
    public GameObject noteTargetPrefab;

    private ObiRope rope;
    private GameObject currentNoteTarget;

    void Start () {
        rope = GetComponent<ObiRope>();
    }
	void Update ()
    {
        int lastParticle = rope.elements[rope.elements.Count - dev_elementIndex].particle2;
        Vector3 lastPosition = rope.solver.positions[lastParticle];
        Vector3 worldPosition = rope.transform.TransformPoint(lastPosition);

        Debug.Log($"lastPosition {lastPosition}, worldPosition {worldPosition}");

        if (currentNoteTarget != null) {
            currentNoteTarget.transform.position = new Vector3(worldPosition.x, worldPosition.y, -0.1f);

        } else {
            currentNoteTarget = Instantiate(noteTargetPrefab, new Vector3(worldPosition.x, worldPosition.y, -0.1f), Quaternion.identity);
        }
        // int lastParticle = rope.elements[rope.elements.Count-1].particle2;
        // var lastPosition = rope.solver.positions[lastParticle];
        // float distance = Vector3.Distance(lastPosition, player.transform.position);
        // Debug.Log($"lastPosition distance {distance}");
	}


    // void OnDrawGizmos() {
    // if (rope != null && player != null) {
    //     int lastParticle = rope.elements[0].particle2;
    //     var lastPosition = rope.transform.TransformPoint(rope.solver.positions[lastParticle]);
    //     Debug.Log($"lastPosition {lastPosition}");
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawSphere(lastPosition, 0.8f);
    //     //Gizmos.DrawLine(lastPosition, player.transform.position);
    // }
}
