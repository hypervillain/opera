using UnityEngine;

public class ObjectTransform2D : MonoBehaviour
{
    void Update()
    {
        transform.position = new Vector3 (transform.position.x, transform.position.y, 0);

        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, 0, currentRotation.eulerAngles.z);
        transform.rotation = targetRotation;
    }
}