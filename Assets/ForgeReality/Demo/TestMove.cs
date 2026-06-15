using UnityEngine;

namespace NecromancerPack
{
    public class TestMove : MonoBehaviour
    {
        [SerializeField] float speed;
        private Animator animator;
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.D))
            {
                if (transform.localScale.x < 0)
                {
                    MirorPlayer();
                }

                transform.Translate(Vector2.right * speed * Time.deltaTime);
                animator.SetBool("IsMove", true);

                IsMoving = true;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                if (transform.localScale.x > 0)
                {
                    MirorPlayer();
                }

                transform.Translate(Vector2.left * speed * Time.deltaTime);
                animator.SetBool("IsMove", true);

                IsMoving = true;
            }
            else
            {
                animator.SetBool("IsMove", false);
                IsMoving = false;
            }
        }

        private void MirorPlayer()
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
    }
}
