using UnityEngine;

public class CameraFreeLook : MonoBehaviour
{
    public float sensitivity = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Opzionale: Nasconde il cursore mentre ti muovi
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // --- ROTAZIONE CON MOUSE ---
        if (Input.GetMouseButton(1)) // Ruota solo tenendo premuto il tasto destro
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Evita che la camera si ribalti

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0);
        }

    }
}