[English](README.md) | [Русский](README.ru.md)

# SDUI — Server-Driven UI for Unity

`com.kiriyo.sdui` · v0.1.0 · by Kiriyo

A Unity package for **Server-Driven UI** — UI described as JSON and rendered at runtime. Enables A/B testing, live updates, and localization without app releases.

> See [RemoteCanvas](https://github.com/kirillrybin/RemoteCanvas) for a full demo.

---

## Requirements

| Dependency | Version |
|---|---|
| Unity | 2022.3+ |
| UniTask | 2.3.3 |
| Newtonsoft.Json | 3.2.1 |


---

## Installation

In Package Manager → Add package from git URL:

```
https://github.com/kirillrybin/com.kiriyo.sdui.git#v0.1.0
```

---

## Architecture

```
Server (JSON)
     │
     ▼
SDUIService          ← fetches pages, TTL cache, loading events
     │
     ▼
UIBuilder            ← clears root, builds component tree recursively
     │
     ▼
ComponentFactory     ← type string → IComponentBuilder (O(1) lookup)
     │
     ▼
IComponentBuilder    ← instantiates prefab, reads JSON fields, wires actions
     │
     ▼
ActionDispatcher     ← decoupled string-based event bus
```

---

## Built-in component types

| Type | Description |
|---|---|
| `button` | Tappable button wired to `ActionDispatcher` |
| `text` | TextMeshPro label |
| `image` | Remote image loaded via URL |
| `banner` | Hero banner with title, subtitle, CTA, and background image |
| `panel` | Layout container supporting nested `children` |
| `spacer` | Fixed-height empty space |
| `news_feed` | Scrollable list fetched from a remote endpoint |

Unknown types render a red fallback placeholder — no crash.

---

## Adding a custom component

```csharp
public class CardBuilder : IComponentBuilder
{
    public string Type => "card";

    public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
    {
        var go = new GameObject("Card", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return UniTask.FromResult(go);
    }
}
```

Register it so that `ComponentFactory` picks it up automatically via `IEnumerable<IComponentBuilder>` — no other changes needed.

---

## DI Setup

### VContainer

```csharp
public class SDUILifetimeScope : LifetimeScope
{
    [SerializeField] 
    private SDUIConfig _config;
    [SerializeField] 
    private string _userId = "player_001";

    [Header("Prefabs")]
    [SerializeField] 
    private Button _buttonPrefab;
    [SerializeField] 
    private TMP_Text _textPrefab;
    [SerializeField] 
    private Image _imagePrefab;
    [SerializeField] 
    private GameObject _bannerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_config);
        builder.RegisterInstance(new PlayerProfile(_userId)).As<IPlayerProfile>();

        // Typed prefab wrappers
        builder.RegisterInstance(new ButtonPrefab { Value = _buttonPrefab });
        builder.RegisterInstance(new TextPrefab { Value = _textPrefab });
        builder.RegisterInstance(new ImagePrefab { Value = _imagePrefab });
        builder.RegisterInstance(new BannerPrefab { Value = _bannerPrefab });

        // Builders are collected into IEnumerable<IComponentBuilder> by the container
        builder.Register<ButtonBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<TextBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ImageBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<PanelBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SpacerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<BannerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<NewsFeedBuilder>(Lifetime.Singleton).AsImplementedInterfaces();

        // Core
        builder.Register<ActionDispatcher>(Lifetime.Singleton);
        builder.Register<ComponentFactory>(Lifetime.Singleton);
        builder.Register<UIBuilder>(Lifetime.Singleton).As<IUIBuilder>();
        builder.Register<SDUIService>(Lifetime.Singleton).As<ISDUIService>();
        builder.Register<SDUINavigator>(Lifetime.Singleton);
        builder.Register<SDUIHttpClient>(Lifetime.Singleton).AsImplementedInterfaces();

        builder.RegisterComponentInHierarchy<SDUIEntryPoint>();
    }
}
```

### Zenject

```csharp
public class SDUIInstaller : MonoInstaller
{
    [SerializeField] private SDUIConfig _config;
    [SerializeField] private string _userId = "player_001";

    [Header("Prefabs")]
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] private TMP_Text _textPrefab;
    [SerializeField] private Image _imagePrefab;
    [SerializeField] private GameObject _bannerPrefab;

    public override void InstallBindings()
    {
        Container.BindInstance(_config);
        Container.Bind<IPlayerProfile>().FromInstance(new PlayerProfile(_userId));

        // Typed prefab wrappers
        Container.BindInstance(new ButtonPrefab { Value = _buttonPrefab });
        Container.BindInstance(new TextPrefab { Value = _textPrefab });
        Container.BindInstance(new ImagePrefab { Value = _imagePrefab });
        Container.BindInstance(new BannerPrefab { Value = _bannerPrefab });

        // Builders — Zenject collects all IComponentBuilder bindings into IEnumerable<IComponentBuilder>
        Container.Bind<IComponentBuilder>().To<ButtonBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<TextBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<ImageBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<PanelBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<SpacerBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<BannerBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<NewsFeedBuilder>().AsSingle();

        // Core
        Container.Bind<ActionDispatcher>().AsSingle();
        Container.Bind<ComponentFactory>().AsSingle();
        Container.Bind<IUIBuilder>().To<UIBuilder>().AsSingle();
        Container.Bind<ISDUIService>().To<SDUIService>().AsSingle();
        Container.Bind<SDUINavigator>().AsSingle();
        Container.Bind<ISDUIHttpClient>().To<SDUIHttpClient>().AsSingle();

        Container.BindInterfacesAndSelfTo<SDUIEntryPoint>().FromComponentInHierarchy();
    }
}
```

---

## License

MIT