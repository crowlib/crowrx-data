# CrowRx.Data.Unity

The Unity integration for the **CrowRx.Data** system. It provides automatic lifecycle-based data binding for `MonoBehaviour` components using **R3**.

This package extends the core [CrowRx.Data](https://github.com/crowlib/crowrx-data) system, which uses a Source-Target pattern to decouple data updates from business logic, to work seamlessly within the Unity lifecycle.

## Dependencies

- **[CrowRx](https://github.com/crowlib/crowrx)**: Base utility package.
- **[R3](https://github.com/Cysharp/R3)**: Reactive extensions for Unity.
- **[UniTask](https://github.com/Cysharp/UniTask)**: Efficient allocation-free async/await integration.

## Installation

### 1. Install Core (Required)
This package requires the core **CrowRx.Data** library. You **must** install it using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) to ensure all dependencies are resolved correctly.

1. Open **NuGet > Manage NuGet Packages** in Unity.
2. Search for `CrowRx.Data`.
3. Click **Install**.

### 2. Install Unity Integration (UPM)
After installing the core library, add this package to your project using the Unity Package Manager:

1. Open **Window > Package Manager**.
2. Click the **+** button and select **Add package from git URL...**.
3. Enter the following URL:
   `https://github.com/crowlib/crowrx-data.git?path=src/CrowRx.Data.Unity/Assets/CrowRx.Data.Unity`

---

## API Reference

### 1. Data Binding (`MonoBehaviourBinder<TTarget>`)
A specialized binder that manages the subscription lifecycle of a `TTarget` based on a `MonoBehaviour`'s state.

#### UnityEventBind (Enum)
Defines when the binding should be active:
- **AwakeAndDestroy**: Binds on `Awake` and unbinds on `OnDestroy`.
- **EnableAndDisable**: Binds on `OnEnable` and unbinds on `OnDisable`.

---

### 2. Extensions (`MonoBehaviourExtension`)
Provides convenient methods to initiate data binding directly from any `MonoBehaviour`.

#### `BindEvent<T>(onUpdate, eventBind)`
Creates and attaches a `MonoBehaviourBinder` to the target `MonoBehaviour`.

- **onUpdate**: Callback invoked whenever the managed data changes.
- **eventBind**: The lifecycle timing for binding (defaults to `EnableAndDisable`).

**Example:**
```csharp
public class HealthUI : MonoBehaviour {
    [SerializeField] private UnityEngine.UI.Slider hpSlider;

    private void Awake() {
        // Automatically binds to PlayerStats changes
        // Subscription starts on Enable and stops on Disable
        this.BindEvent<PlayerStats>(stats => {
            hpSlider.value = stats.HP / stats.MaxHP;
        }, UnityEventBind.EnableAndDisable);
    }
}
```

## Requirements
- **Unity 6000.3 or newer**
- **CrowRx.Data** (Core library installed via NuGet)

## License
This project is licensed under the [MIT License](LICENSE).
