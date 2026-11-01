
using UnityEngine;

public class ForgyFloating : MonoBehaviour
{
    public float floatHeight = 0.05f;
    public float floatSpeed = 1.5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.localPosition = startPosition + Vector3.up * offset;
    }
}
