using UnityEngine;

namespace NecromancerPack
{
    public class TestDie : MonoBehaviour
    {
        private Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.SetTrigger("Hit");
                anim.SetInteger("Hp", 0);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                anim.SetTrigger("Return");
            }
        }
    }
}
