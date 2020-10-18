using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Entities
{
    public class Entity : MonoBehaviour
    {
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

        [Header("Settings")]
        [SerializeField] private float maxHealth;
        public float Speed;
        public Transform Target;

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

        protected virtual void HandleMovement()
        {

        }

        protected virtual void HandleMovementTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.position, Speed * Time.fixedDeltaTime);
        }

        protected virtual void FindTarget()
        {

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

        private void FixedUpdate()
        {
            if (Target)
                HandleMovementTarget();
            else
                HandleMovement();
        }

        #endregion
    }
}
