using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Health")]
    public bool IsAlive = true;
    public int MaxHealth = 100;
    public int CurrentHealth = 100;

    // events
    public delegate void DamageHandler();
    public event DamageHandler OnDamage;

    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int dmg)
    {
        if (IsAlive)
        {
            CurrentHealth -= dmg;
            OnDamage.Invoke();
            // TODO some feedback (red, flashing, idk)
            Debug.Log($"{name} - Damage Taken @ {Time.time}");

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsAlive = false;
                // TODO die!
            }
        }
    }
}
