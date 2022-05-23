public class HealthBar : UIBar
{
    public Actor Actor;

    private void Start()
    {
        float healthPercent = (float)Actor.CurrentHealth / Actor.MaxHealth;
        SetBarPercent(healthPercent);
        Actor.OnDamage += UpdateHealthBar;
    }

    private void OnDisable()
    {
        Actor.OnDamage -= UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        float healthPercent = (float)Actor.CurrentHealth / Actor.MaxHealth;
        SetBarPercent(healthPercent);
    }
}
