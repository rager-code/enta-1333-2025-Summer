using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_RTS : MonoBehaviour
{

    public float moveSpeed = 20f;       // Speed of camera movement
    public float rotateSpeed = 100f;    // Speed of camera rotation
    public float zoomSpeed = 15f;        // Speed of zooming in and out
    public float minZoom = 5f;          // Minimum zoom limit
    public float maxZoom = 60;        // Maximum zoom limit
    public float verticalSpeed = 5f;    // Speed of camera vertical movement

    private Camera cam;

    void Start()
    {
        // Cache reference to the main camera
        cam = Camera.main;
    }

    void Update()
    {
        // Store current position
        Vector3 pos = transform.position;

        // Forward movement
        if (Input.GetKey("w"))
        {
            pos.z += moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Backward movement
        if (Input.GetKey("s"))
        {
            pos.z -= moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Right movement
        if (Input.GetKey("d"))
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Left movement
        if (Input.GetKey("a"))
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Move the camera down
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
        }

        // Move the camera up
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
        }

        // Zoom in/out with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);

        // Set zoom to max
        if (Input.GetKey(","))
        {
            cam.fieldOfView = 60f;
        }

        // Set zoom to min
        if (Input.GetKey("."))
        {
            cam.fieldOfView = 5f;
        }
    }
}

