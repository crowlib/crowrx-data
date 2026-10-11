# CrowRx.Data

A high-performance, reactive data broker system designed for Unity, utilizing the Source-Target pattern to decouple data updates from business logic.

## Core Concepts

- **Source (ISource)**: A data object representing a new state or update.
- **Target (ITarget<TSource>)**: A managed object that consumes specific ISource types to update its internal state.
- **Broker**: The central hub that routes ISource updates to all associated ITarget instances.
- **Managed (Managed<TTarget>)**: A wrapper that manages ITarget instances and provides reactive streams via R3.Observable.

## API Reference

### 1. Data Interfaces

#### ISource
A marker interface for data source objects.

#### ITarget<TSource>
Defines an object that can be updated by a specific TSource.

- **bool UpdateBy(in TSource sourceData)**: Updates the target's state. Returns 	rue if observers should be notified of the change.

**Example:**
```csharp
public struct PlayerHealthSource : ISource {
    public float CurrentHP;
}

public class PlayerStats : ITarget<PlayerHealthSource> {
    public float HP { get; private set; }
    
    public bool UpdateBy(in PlayerHealthSource source) {
        if (HP == source.CurrentHP) return false;
        HP = source.CurrentHP;
        return true; // Broadcast change
    }
}
```

---

### 2. Central Hub (Broker Static Class)

Manages the flow of data across the system.

- **Broker.UpdateBy(ISource source)**: Updates all targets associated with the given source.
- **Broker.UpdateBy(IEnumerable<ISource> sources)**: Batch updates for multiple sources.
- **Broker.Release()**: Disposes of all managed data and targets.

**Example:**
```csharp
// Single update
Broker.UpdateBy(new PlayerHealthSource { CurrentHP = 80f });

// Batch update
Broker.UpdateBy(new ISource[] { 
    new PlayerHealthSource { CurrentHP = 100f },
    new PlayerExpSource { CurrentExp = 500 }
});
```

---

### 3. Instance Management (Managed<TTarget> Static Class)

Provides access to managed target instances and their change streams.

- **Managed<TTarget>.Instance**: Accesses the globally managed singleton instance of TTarget.
- **Managed<TTarget>.Observable**: Returns an R3.Observable<TTarget> that triggers whenever the target state changes.

**Example:**
```csharp
// Access the instance
float currentHP = Managed<PlayerStats>.Instance.HP;

// Subscribe to changes
Managed<PlayerStats>.Observable.Subscribe(stats => {
    Debug.Log($"HP changed to: {stats.HP}");
});
```

---

### 4. Data Binding (Binder<TTarget> Abstract Class)

A utility class for binding managed data to UI or other components.

- **Bind()**: Starts listening to target changes.
- **Unbind()**: Stops listening to target changes.

**Example:**
```csharp
public class HealthBarBinder : Binder<PlayerStats> {
    public HealthBarBinder(Action<PlayerStats> onUpdate) : base(onUpdate) { }
    public override void Dispose() => Unbind();
}
```

## Requirements
- **Unity 6.0 or newer**
- **R3**: Reactive extensions for Unity.
- **CrowRx.Core**: Base utilities.

## Installation
Install via NuGet or as a UPM package. This library is designed to work seamlessly with CrowRx.Data.SourceGenerator for automatic Couple registration.

## License
This project is licensed under the [MIT License](LICENSE).
