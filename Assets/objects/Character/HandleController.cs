using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class HandleController : MonoBehaviour
{
	public float minDistanceHandleToPlayer = 1f;
    public float speed = 1;

    public ObiRope[] ropes;
    public GameObject handle;
    public GameObject player;

    private ObiRopeCursor[] cursors;

    private float _initialRestLength;
    private float _initialDistanceHandleToPlayer;

    void Start () {
        cursors = new ObiRopeCursor[ropes.Length];
        for (int i = 0; i < ropes.Length; i++) {
            cursors[i] = ropes[i].GetComponent<ObiRopeCursor>();
        }
        _initialRestLength = ropes[0].restLength;

        _initialDistanceHandleToPlayer = Vector3.Distance(handle.transform.position, player.transform.position);
    }
	void Update ()
    {
        float distance = Vector3.Distance(handle.transform.position, player.transform.position);
        float finalDistance = Mathf.Clamp(distance, minDistanceHandleToPlayer, _initialDistanceHandleToPlayer);
        Debug.Log($"distance {distance} {ropes[0].restLength}");
        for (int i = 0; i < cursors.Length; i++)
        {
            cursors[i].ChangeLength(distance);
        }

        // float h = Input.GetAxis("Vertical");
        // for (int i = 0; i < cursors.Length; i++)
        // {
        //     if (ropes[i].restLength > minLength || ropes[i].restLength < initialRestLength)
        //     {   
        //         cursors[i].ChangeLength(ropes[i].restLength + (-h * speed * Time.deltaTime));
        //     }
        // }
	}
}
