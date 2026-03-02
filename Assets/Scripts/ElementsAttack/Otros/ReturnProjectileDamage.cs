using UnityEngine;
using Pizzard.Bosses;

public class ReturnProjectileDamage : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
        {
            BossBase boss = other.GetComponent<BossBase>();
            if (boss == null) boss = other.GetComponentInParent<BossBase>();
            
            if (boss != null)
            {
                boss.TakeDamage((int)damage);
            }
            else
            {
                PblobController pblob = other.GetComponent<PblobController>();
                if (pblob != null)
                {
                    pblob.TakeDamage(damage);
                }
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
