using UnityEngine;
using System;

public class Restart : MonoBehaviour
{
    public static event Action OnRestartClicked;
    void Start()
    {

    }

    void OnMouseDown()
    {
        OnRestartClicked?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
