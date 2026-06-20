# Enemy System Documentation

This is a modular enemy system for Unity 2D games compatible with the new input system.

## Components

### 1. **EnemyHealth.cs**
Handles all health-related functionality for enemies.

**Features:**
- Health management (damage, healing, death)
- Events for health changes, damage taken, and death
- Death effects and sounds
- Damage flash visual feedback
- Configurable max health

**Usage:**
```csharp
// Get reference to component
EnemyHealth health = enemy.GetComponent<EnemyHealth>();

// Take damage
health.TakeDamage(10f);

// Heal
health.Heal(5f);

// Subscribe to events
health.OnDeath += HandleEnemyDeath;
health.OnHealthChanged += (current, max) => UpdateHealthBar(current, max);
```

**Inspector Settings:**
- `maxHealth` - Maximum health of the enemy
- `deathEffectPrefab` - Prefab to spawn on death
- `deathDelay` - Delay before enemy is destroyed
- `deathSoundClip` - Audio played on death
- `damageSoundClip` - Audio played when taking damage

---

### 2. **Enemy.cs**
Base enemy controller with AI behavior (chasing, attacking).

**Features:**
- Player detection and chasing
- Attack system with cooldown
- Animation management
- Movement using Rigidbody2D
- Sprite flipping based on direction
- Configurable detection and attack ranges

**Usage:**
```csharp
// Configure enemy
Enemy enemy = GetComponent<Enemy>();
enemy.SetMoveSpeed(4f);
enemy.SetDetectionRange(8f);
enemy.SetAttackDamage(10f);
enemy.SetPlayerTransform(playerTransform);

// Get health component
EnemyHealth health = enemy.GetHealth();
```

**Inspector Settings:**
- `moveSpeed` - Speed of enemy movement
- `detectionRange` - Distance to detect player
- `playerTransform` - Reference to player (auto-found if tagged "Player")
- `attackCooldown` - Time between attacks
- `attackRange` - Distance to attack
- `attackDamage` - Damage per attack
- Animation names for idle, move, and attack

**Requirements:**
- SpriteRenderer component
- Animator component
- Rigidbody2D component
- Player must have "Player" tag and PlayerHealth component

---

### 3. **EnemySpawner.cs**
Manages enemy spawning with a wave system.

**Features:**
- Wave-based spawning
- Configurable spawn radius
- Max active enemies limit
- Wave events (start, complete)
- Auto cleanup of destroyed enemies
- Automatic player detection

**Usage:**
```csharp
// Start spawning
EnemySpawner spawner = GetComponent<EnemySpawner>();
spawner.StartSpawning();

// Add waves via inspector or code
spawner.AddWave(enemyCount: 5, spawnDelay: 1f, waveDelay: 3f);

// Subscribe to events
spawner.OnWaveStart += (waveNumber) => Debug.Log($"Wave {waveNumber} started");
spawner.OnWaveComplete += (waveNumber) => Debug.Log($"Wave {waveNumber} complete");
spawner.OnAllWavesComplete += () => Debug.Log("All waves complete");

// Stop or clear
spawner.StopSpawning();
spawner.ClearAllEnemies();

// Get info
int activeCount = spawner.GetActiveEnemyCount();
```

**Inspector Settings:**
- `enemyPrefab` - Enemy prefab to spawn
- `spawnPoint` - Transform to spawn from
- `spawnRadius` - Radius around spawn point
- `waves` - List of waves with settings
- `autoStart` - Start spawning on scene load
- `maxEnemiesActive` - Maximum enemies allowed at once

**Wave Settings:**
- `enemyCount` - How many enemies to spawn
- `spawnDelay` - Delay between each enemy spawn
- `waveDelay` - Delay before next wave starts

---

## Setup Instructions

### 1. Create Enemy Prefab
1. Create a new GameObject named "Enemy"
2. Add required components:
   - **SpriteRenderer** - Add your enemy sprite
   - **Animator** - Create an animator controller with states: "Idle", "Move", "Attack"
   - **Rigidbody2D** - Set Body Type to Dynamic, Gravity Scale to 0, Constraints: Freeze Rotation Z
   - **CircleCollider2D** - For collision detection
3. Add scripts:
   - **EnemyHealth** component
   - **Enemy** component
4. Configure settings in inspector
5. Save as prefab in Assets/Prefabs/

### 2. Setup Spawner
1. Create an empty GameObject named "EnemySpawner"
2. Add **EnemySpawner** script
3. Assign the Enemy prefab to `enemyPrefab`
4. Create a child object as spawn point and assign it
5. Configure waves in inspector
6. Ensure Player has "Player" tag and **PlayerHealth** component

### 3. Player Setup
Player must have:
- "Player" tag
- PlayerHealth component with `TakeDamage(float)` method

---

## Events Reference

### EnemyHealth Events
```csharp
OnHealthChanged?.Invoke(currentHealth, maxHealth);
OnDeath?.Invoke();
OnTakeDamage?.Invoke();
```

### EnemySpawner Events
```csharp
OnWaveStart?.Invoke(waveNumber);
OnWaveComplete?.Invoke(waveNumber);
OnAllWavesComplete?.Invoke();
```

---

## Example Setup

```csharp
// In your GameManager or similar
public class GameManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;

    private void Start()
    {
        // Setup wave callbacks
        enemySpawner.OnWaveStart += OnWaveStart;
        enemySpawner.OnAllWavesComplete += OnGameComplete;

        // Start spawning
        enemySpawner.StartSpawning();
    }

    private void OnWaveStart(int waveNumber)
    {
        Debug.Log($"Wave {waveNumber} started! Active enemies: {enemySpawner.GetActiveEnemyCount()}");
    }

    private void OnGameComplete()
    {
        Debug.Log("All waves defeated! You win!");
    }
}
```

---

## Customization Tips

- **Different Enemy Types:** Create derived classes from `Enemy.cs` with different behavior
- **Boss Enemies:** Increase health via `EnemyHealth.SetMaxHealth()` and customize `Enemy` behavior
- **Difficulty Scaling:** Adjust wave settings, damage, and health based on progression
- **Animations:** Ensure animator controller has the animation states referenced in inspector
