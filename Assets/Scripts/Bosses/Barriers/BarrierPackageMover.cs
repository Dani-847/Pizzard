using UnityEngine;

namespace Pizzard.Bosses.Barriers
{
    /// <summary>
    /// Moves a grouped package of barrier dots together.
    /// Can oscillate or move linearly.
    /// </summary>
    public class BarrierPackageMover : MonoBehaviour
    {
        public Vector3 velocity;
        public float lifetime = 15f;
        public bool oscillate = false;
        public float oscSpeed = 1f;
        public float oscAmount = 3f;
        
        private Vector3 startPos;
        
        private void Start()
        {
            startPos = transform.position;
            if (lifetime > 0)
                Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (oscillate)
            {
                transform.position = startPos + velocity.normalized * (Mathf.Sin(Time.time * oscSpeed) * oscAmount);
            }
            else
            {
                transform.position += velocity * Time.deltaTime;
            }
        }
    }
}
