using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
public class MouseLook : MonoBehaviour
{
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(100f, 100f);

    private Transform cameraTransform;
    private float rotationX;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(Application.isEditor)
            mouseSensitivity *= 2.2f;
    }

    void Update()
    {
        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        float x = Input.GetAxis("Mouse X") * mouseSensitivity.x * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * mouseSensitivity.y * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX - y, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * x);
    }
}
