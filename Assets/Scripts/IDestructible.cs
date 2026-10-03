public interface IDestructible
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    void TakeDamage(float damageAmount);
}