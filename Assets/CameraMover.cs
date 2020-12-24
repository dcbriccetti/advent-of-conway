using UnityEngine;

public class CameraMover : MonoBehaviour
{
    private Vector3 startingPos;

    void Start() {
        startingPos = transform.position;
    }

    private void OnPostRender() {
        transform.position = startingPos + Vector3.right * (1 * Mathf.Sin(Time.time));
    }
}
