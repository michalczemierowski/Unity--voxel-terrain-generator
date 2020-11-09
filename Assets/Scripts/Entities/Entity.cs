using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities
{
    /// <summary>
    /// Base class for stationary entities
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [SerializeField] private float maxHealth;
        private float health;
        public float Health
        {
            get
            {
                return health;
            }
            set
            {
                float healthBefore = health;
                health = value;

                if (healthBefore > health)
                    OnDamage(healthBefore, health);
                else if (healthBefore < health)
                    OnHeal(healthBefore, health);
            }
        }

        protected virtual void OnSpawn()
        {
            health = maxHealth;
        }

        protected virtual void OnDamage(float healthBefore, float healthNow)
        {
            if (healthNow <= 0)
                OnDeath(healthBefore, healthNow);
        }

        protected virtual void OnHeal(float healthBefore, float healthNow)
        {

        }

        protected virtual void OnDeath(float healthBefore, float healthNow)
        {
            Destroy(gameObject);
        }

        #region  public methods

        public void Damage(float damage)
        {
            Health -= damage;
        }

        #endregion

        #region private methods

        private void Start()
        {
            OnSpawn();
        }

        #endregion
    }
}
