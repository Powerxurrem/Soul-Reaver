using UnityEngine;

namespace NecromancerPack
{
    public class Parallax : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] public float parallaxFactor = 0.5f;

        private Transform cam;
        private Vector3 lastCamPos;

        private float width;
        private Transform[] parts;

        void Awake()
        {
            cam = Camera.main.transform;
            lastCamPos = cam.position;

            int count = transform.childCount;
            parts = new Transform[count];
            for (int i = 0; i < count; i++)
                parts[i] = transform.GetChild(i);

            width = parts[0].GetComponent<SpriteRenderer>().bounds.size.x;
        }

        void Update()
        {
            Vector3 delta = cam.position - lastCamPos;
            lastCamPos = cam.position;

            transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);

            foreach (var part in parts)
            {
                if (cam.position.x - part.position.x > width)
                {
                    float rightmostX = GetRightmostX();
                    part.position = new Vector3(rightmostX + width, part.position.y, part.position.z);
                }
                else if (part.position.x - cam.position.x > width)
                {
                    float leftmostX = GetLeftmostX();
                    part.position = new Vector3(leftmostX - width, part.position.y, part.position.z);
                }
            }
        }

        float GetRightmostX()
        {
            float max = parts[0].position.x;
            foreach (var part in parts)
                if (part.position.x > max) max = part.position.x;
            return max;
        }

        float GetLeftmostX()
        {
            float min = parts[0].position.x;
            foreach (var part in parts)
                if (part.position.x < min) min = part.position.x;
            return min;
        }
    }
}
