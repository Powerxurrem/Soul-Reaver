using UnityEngine;

namespace NecromancerPack
{
    public class TargetCam : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float speed;

        void LateUpdate()
        {
            Vector3 desiredPosition = target.position + offset;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);

            transform.position = smoothedPosition;
        }
    }
}
