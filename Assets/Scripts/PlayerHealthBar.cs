public class PlayerHealthBar : UIBar
{
    public PlayerController Player;

    private void Start()
    {
        float healthPercent = (float)Player.CurrentHealth / Player.MaxHealth;
        SetBarPercent(healthPercent);
        Player.OnDamage += UpdateHealthBar;
    }

    private void OnDisable()
    {
        Player.OnDamage -= UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        float healthPercent = (float)Player.CurrentHealth / Player.MaxHealth;
        SetBarPercent(healthPercent);
    }
}
