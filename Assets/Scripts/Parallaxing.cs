using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxing : MonoBehaviour
{
    public Transform[] backgrounds;                     // Array of all the back and foregrounds to be parallaxed.
    private float[] parallaxScales;                     // The proportion of the camera's movement to move the backgrounds by.
    private const float smoothing = 5.0f;               // How smooth the parallax is going to be. Make sure to set this above 0.

    private new Transform camera;                       // Reference to the main camera's transform.
    private Vector3 previousCameraPosition;             // The positin of the camera in the previous frame.

    // Awake is called before Start(). Great for references.
    private void Awake()
    {
        // Set up the camera reference.
        camera = Camera.main.transform;
    }

    // Start is called before the first frame update.
    void Start()
    {
        // The previous frame had the current frame's camera position.
        previousCameraPosition = camera.position;

        // Assigning corresponding parallaxScales.
        parallaxScales = new float[backgrounds.Length];

        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxScales[i] = backgrounds[i].position.z * -1;
        }
    }

    // Update is called once per frame.
    void Update()
    {
        for (int i =0; i < backgrounds.Length; i++)
        {
            float parallax = (previousCameraPosition.x - camera.position.x) * parallaxScales[i];

            // Set a target x position which is the current position plus the parallax.
            float backgroundTargetPositionX = backgrounds[i].position.x + parallax;

            // Create a target position which is the background's current position with its target x position
            Vector3 backgroundTargetPosition = new(backgroundTargetPositionX, backgrounds[i].position.y, backgrounds[i].position.z);

            // Fade between current position and the target position using lerp.
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPosition, smoothing * Time.deltaTime);
        }

        // Set the previousCameraPosition to the camera's position at the end of the frame.
        previousCameraPosition = camera.position;
    }
}
