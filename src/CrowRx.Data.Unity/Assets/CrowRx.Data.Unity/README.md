# CrowRx.Data.Unity

The Unity integration for the CrowRx.Data system. It provides automatic lifecycle-based data binding for MonoBehaviour components using R3.

## Dependencies

- **com.crowlib.crowrx**: Base utility package.
- **com.cysharp.r3**: Reactive extensions for Unity.
- **com.cysharp.unitask**: Async/Await integration.

## API Reference

### 1. Data Binding (MonoBehaviourBinder<TTarget>)
A specialized binder that manages the subscription lifecycle of a TTarget based on a MonoBehaviour's state.

#### UnityEventBind (Enum)
Defines when the binding should be active:
- **AwakeAndDestroy**: Binds on creation and unbinds on destruction.
- **EnableAndDisable**: Binds when enabled and unbinds when disabled.

---

### 2. Extensions (MonoBehaviourExtension)
Provides convenient methods to initiate data binding directly from any MonoBehaviour.

#### BindEvent<T>(onUpdate, eventBind)
Creates and attaches a MonoBehaviourBinder to the target MonoBehaviour.

- **onUpdate**: Callback invoked whenever the managed data changes.
- **eventBind**: The lifecycle timing for binding.

**Example:**
`csharp
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
`

## Requirements
- **Unity 6000.3 or newer**
- **CrowRx.Data**: The core data broker system.

## Installation
Add the package to your manifest.json or install via the Package Manager using the Git URL.

## License
This project is licensed under the [MIT License](LICENSE).
