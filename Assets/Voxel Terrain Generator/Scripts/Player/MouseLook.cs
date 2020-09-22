using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
public class MouseLook : MonoBehaviour
{
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(100f, 100f);
    [SerializeField] private float minimumRotationY;
    [SerializeField] private float maximumRotationY;

    [System.NonSerialized]
    public static Transform cameraTransform;
    private Quaternion cameraOriginalRotation;
    private Quaternion transformOriginalRotation;

    private float rotationY;
    private float rotationX;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(Application.isEditor)
            mouseSensitivity *= 2.2f;

        cameraOriginalRotation = cameraTransform.localRotation;
        transformOriginalRotation = transform.rotation;
    }

    void Update()
    {
        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity.x;
        rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity.y;

        rotationY = ClampAngle(rotationY, minimumRotationY, maximumRotationY);

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

        cameraTransform.localRotation = cameraOriginalRotation * yQuaternion;
        transform.rotation = transformOriginalRotation * xQuaternion;//new Vector3(transform.localEulerAngles.x, cameraTransform.eulerAngles.y, transform.localEulerAngles.z);

        //rotationY = Input.GetAxis("Mouse X") * mouseSensitivity.x * Time.fixedDeltaTime;
        //float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity.y * Time.fixedDeltaTime;

        //rotationX = Mathf.Clamp(rotationX - mouseY, -90f, 90f);

        //cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        //transform.Rotate(Vector3.up * rotationY);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
