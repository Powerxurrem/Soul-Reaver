using UnityEngine;

/// <summary>
/// CloakBone2D – script for animating a cloak/robe bone in 2D.
/// - When the character is idle → bone swings gently like in the wind.
/// - When the character moves → the cloak is pushed back, simulating air resistance with turbulence.
/// Attach this script to your character and assign the targetBone.
/// </summary>
namespace NecromancerPack
{
    public class CloakBone2D : MonoBehaviour
    {
        [Header("The bone to animate")]
        [SerializeField] private Transform targetBone;

        [Header("Wind (Idle Mode)")]
        [SerializeField] private float windStrength = 10f;   // Swing angle amplitude (degrees)
        [SerializeField] private float windSpeed = 2f;       // Speed of wind oscillation

        [Header("Movement (When Character Moves)")]
        [SerializeField] private float moveInfluence = 25f;   // How much movement affects the angle
        [SerializeField] private float returnSpeed = 5f;      // Speed of returning to base rotation

        [Header("Turbulence (Extra sway while moving)")]
        [SerializeField] private float turbulenceStrength = 5f; // Amplitude of turbulence
        [SerializeField] private float turbulenceSpeed = 15f;   // Speed of turbulence oscillation

        [Header("General")]
        [SerializeField] private float movementThreshold = 0.05f; // Min speed to count as movement

        private Vector3 lastPos;
        private float windOffset;
        private float turbulenceOffset;
        private float baseAngle;

        private void Start()
        {
            if (targetBone == null) return;

            lastPos = transform.position;
            windOffset = Random.value * 10f;       // Random offset for idle wind
            turbulenceOffset = Random.value * 10f; // Random offset for turbulence
            baseAngle = targetBone.localEulerAngles.z;
        }

        private void Update()
        {
            if (targetBone == null) return;

            // Calculate movement delta
            Vector3 delta = transform.position - lastPos;
            float speed = delta.magnitude / Time.deltaTime;

            float targetAngle;

            if (speed > movementThreshold)
            {
                // Character is moving → cloak bends backwards (air resistance)
                float moveAngle = -delta.normalized.x * moveInfluence;

                // Add turbulence (like cloak fluttering in the wind)
                float turbulence = Mathf.Sin(Time.time * turbulenceSpeed + turbulenceOffset) * turbulenceStrength;

                targetAngle = baseAngle + moveAngle + turbulence;
            }
            else
            {
                // Character is idle → idle wind sway
                float wind = Mathf.Sin(Time.time * windSpeed + windOffset) * windStrength;
                targetAngle = baseAngle + wind;
            }

            // Smooth transition towards target angle
            float current = Mathf.LerpAngle(targetBone.localEulerAngles.z, targetAngle, Time.deltaTime * returnSpeed);
            targetBone.localRotation = Quaternion.Euler(0, 0, current);

            lastPos = transform.position;
        }
    }
}
