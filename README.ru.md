# SDUI — Server-Driven UI для Unity

`com.kiriyo.sdui` · v0.1.0 · by Kiriyo

Пакет реализует паттерн **Server-Driven UI**: структура и содержимое интерфейса определяются сервером в виде JSON и рендерятся в runtime без изменений клиента.

> Демо-проект: [RemoteCanvas](https://github.com/kirillrybin/RemoteCanvas)

[🇬🇧 Read in English](README.md)

---

## Зависимости

| Пакет | Версия |
|---|---|
| Unity | 2022.3+ |
| UniTask | 2.3.3 |
| Newtonsoft.Json | 3.2.1 |

---

## Установка

Package Manager → Add package from git URL:

```
https://github.com/kirillrybin/com.kiriyo.sdui.git#v0.1.0
```

---

## Архитектура

```
Server (JSON)
     │
     ▼
SDUIService          ← HTTP-запросы, TTL-кэш, события загрузки
     │
     ▼
UIBuilder            ← очистка root, рекурсивный обход дерева компонентов
     │
     ▼
ComponentFactory     ← маппинг type → IComponentBuilder, O(1)
     │
     ▼
IComponentBuilder    ← создание GameObject, чтение полей JSON
     │
     ▼
ActionDispatcher     ← событийная шина на строковых ключах
```

---

## Встроенные компоненты

| Тип | Описание |
|---|---|
| `button` | Кнопка, привязанная к `ActionDispatcher` |
| `text` | TextMeshPro-текст |
| `image` | Изображение по URL |
| `banner` | Баннер: заголовок, подзаголовок, CTA, фоновое изображение |
| `panel` | Контейнер с поддержкой вложенных `children` |
| `spacer` | Отступ фиксированной высоты |
| `news_feed` | Список новостей, загружаемый с сервера |

Неизвестный тип — красный плейсхолдер, без исключения.

---

## Пользовательский компонент

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

`ComponentFactory` подхватывает реализацию автоматически через `IEnumerable<IComponentBuilder>`.

---

## Настройка DI

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

        // Типизированные обёртки над префабами
        builder.RegisterInstance(new ButtonPrefab { Value = _buttonPrefab });
        builder.RegisterInstance(new TextPrefab { Value = _textPrefab });
        builder.RegisterInstance(new ImagePrefab { Value = _imagePrefab });
        builder.RegisterInstance(new BannerPrefab { Value = _bannerPrefab });

        // Builders собираются контейнером в IEnumerable<IComponentBuilder>
        builder.Register<ButtonBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<TextBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ImageBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<PanelBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<SpacerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<BannerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();

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

        // Типизированные обёртки над префабами
        Container.BindInstance(new ButtonPrefab { Value = _buttonPrefab });
        Container.BindInstance(new TextPrefab { Value = _textPrefab });
        Container.BindInstance(new ImagePrefab { Value = _imagePrefab });
        Container.BindInstance(new BannerPrefab { Value = _bannerPrefab });

        // Builders — Zenject собирает все IComponentBuilder в IEnumerable<IComponentBuilder>
        Container.Bind<IComponentBuilder>().To<ButtonBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<TextBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<ImageBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<PanelBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<SpacerBuilder>().AsSingle();
        Container.Bind<IComponentBuilder>().To<BannerBuilder>().AsSingle();

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

## Лицензия

MIT