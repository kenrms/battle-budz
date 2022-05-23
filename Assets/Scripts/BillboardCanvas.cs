using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    public Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    private void LateUpdate()
    {
        transform.LookAt(canvas.worldCamera.transform);
    }
}
