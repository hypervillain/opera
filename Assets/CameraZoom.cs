using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minZoomDistance = -100f;
    public float maxZoomDistance = 10f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.z += scroll * zoomSpeed;
        pos.z = Mathf.Clamp(pos.z, minZoomDistance, maxZoomDistance);
        transform.position = pos;
    }
}