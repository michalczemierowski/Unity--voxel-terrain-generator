/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

public class Entity : MonoBehaviour
{
    private int health;
    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            int healthBefore = health;
            health = value;
            
            if(healthBefore < health)
                OnDamage(healthBefore, health);
            else if(healthBefore > health)
                OnHeal(healthBefore, health);
        }
    }
    public float Speed;
    public Transform Target;

    protected virtual void OnSpawn()
    {

    }

    protected virtual void OnDamage(int healthBefore, int healthNow)
    {
        if(healthNow <= 0)
            OnDeath(healthBefore, healthNow);
    }

    protected virtual void OnHeal(int healthBefore, int healthNow)
    {

    }

    protected virtual void OnDeath(int healthBefore, int healthNow)
    {

    }

    protected virtual void HandleMovement()
    {

    }

    protected virtual void FindTarget()
    {
        
    }

    #region  public methods

    public void Damage(int damage)
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
        HandleMovement();
    }

    #endregion
}
