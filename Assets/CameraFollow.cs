using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{

    [Header("Target Settings")]
    // The transform (usually the player) the camera will follow
    public Transform target;

    [Header("Follow Settings")]
    // Offset from the target position so the camera is not exactly centered on the player
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    // Smoothness factor for camera movement, smaller values = snappier camera
    [Range(0.01f, 1f)] public float smoothTime = 0.2f;

    // Whether the camera follows the target on the X axis
    public bool followX = true;

    // Whether the camera follows the target on the Y axis
    public bool followY = true;

    [Header("Bound (World Units)")]
    // Enable or disable camera bounds clamping
    public bool useBounds = false;

    // Minimum world coordinates for the camera's position (bottom-left corner)
    public Vector2 minBounds;
    // Maximum world coordinates for the camera's position (top-right corner)
    public Vector2 maxBounds;

    [Header("Pixel Perfect Settings")]
    // Whether to snap camera position to pixel grid (useful for pixel art game)
    public bool pixelPerfectSnap = false;

    // Pixels per unit (Matches your sprite PPU (eg., 16 or 32)
    public float pixelPerUnit = 16f;

    // Internal variable to keep track of camera velocity for SmoothDamp
    private Vector3 velocity = Vector3.zero;


    // Variables for camera shake effect
    private Vector3 shakeOffset = Vector3.zero; // Offset for shaking
    private float shakeDuration = 0f; // How long shake lasts
    private float shakeMagnitude = 0.1f; // Intensity of the shake
    private float shakeDampingSpeed = 1f; // How fast shake fades out

    // Called every frame after all Update()calls, to make camera follow smooth
    void LateUpdate()
    {
        // If no target assigned, do nothing
        if (target == null) return;

        // Calculate the desired camera position based on target position plus offset
        Vector3 desiredPosition = target.position + offset;

        if (!followX)
            desiredPosition.x = transform.position.x;

        if (!followY)
            desiredPosition.y = transform.position.y;

        // Smoothly move the camera towards the desired positon using SmoothDamp
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        if (useBounds)
        {

            // Get the main camera component (should be orthographic for 2D)
            Camera cam = Camera.main;
            if (cam.orthographic)
            {
                // Calculate half the camera's height based on orthographic size
                float camHeight = cam.orthographicSize;

                // Calculate half the camera's width using aspect ratio
                float camWidth = camHeight * cam.aspect;

                // Clamp X position between min and max bounds, accounting
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);

                // Clamp Y position between min and max bounds, accounting for camera height
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y + camWidth, maxBounds.y - camWidth);
            }
        }
        // If shake effect is active, add a random offset inside a unit sphere multiplied by magnitude
        if (shakeDuration > 0)
        {
            // Generate random 3D vector inside a sphere for shake offset
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;

            // Zero out Z axis because this is a 2D game
            shakeOffset.z = 0;

            // Decrease Shake duration over time, scaled by damping speed
            shakeDuration -= Time.deltaTime * shakeDampingSpeed;
        }
        else
        {
            // When shake finishes, reset variables
            shakeDuration = 0f;
            shakeOffset = Vector3.zero;
        }

        // Add the shake offset to the camera's position
        smoothedPosition += shakeOffset;

        // if pixel perfect snapping is enabled, snap camera position to the pixel grid
        if (pixelPerfectSnap)
            smoothedPosition = PixelPerfectCamera(smoothedPosition);

        // Finally, update the camera's position to the smoothed and adjusted position
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Snapps a positon vector to the pixel grid to avoid sub-pixel movement.
    /// </summery>
    /// <param name="position">The original position. </param>
    /// <returns>Snapped position aligned to pixel grid.</returns>
    /// 

    private Vector3 PixelPerfectCamera(Vector3 position)
    {
        // Calculate snapped X by rounding to nearest pixel based on pixels per unit
        float snappedX = Mathf.Round(position.x * pixelPerUnit) / pixelPerUnit;

        // Calculate snapped Y by rounding to nearest pixel based on pixels per unit
        float snappedY = Mathf.Round(position.y * pixelPerUnit) / pixelPerUnit;

        // Z position remains unchanged
        float snappedZ = position.z;

        // Return the new snapped position vector
        return new Vector3(snappedX, snappedY, snappedZ);
    }

    /// <summary>
    ///  Public method to trigger the camera shake effect
    /// Can be called from other scripts, e.g. when player takes damage.
    /// <r/summary>
    /// <param name="duration">Duration of the shake in seconds.</param>
    /// <param name="magnitude"=Intensify (amplitude) of the shake. </param>
    /// <param name="dampingSpeed">Speed at which the shake fades out (default = 1). </param>
    public void ShakeCamera(float duration, float magnitude, float dampingSpeed = 1f)
    {
        //Set Shake duration and parameters based on input
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeDampingSpeed = dampingSpeed;
    } 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
