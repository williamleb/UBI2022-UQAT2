using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HUDScreeSpaceCamera : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private float planeDistance = 10f;

    void Start()
    {
        canvas = GetComponent<Canvas>();

        if(canvas == null)
        {
            Debug.LogWarning("No canvas on gameObject. Not showing HUD.");
            return;
        }
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.planeDistance = planeDistance;
        canvas.worldCamera = Camera.main;
    }
}
