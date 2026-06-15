using UnityEngine;

namespace NecromancerPack
{
    public class TestAttack : MonoBehaviour
    {
        private Animator m_Animator;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_Animator.SetTrigger("Attack");
            }
        }
    }
}
