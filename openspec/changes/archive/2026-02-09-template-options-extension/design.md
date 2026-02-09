# Template Options Extension - 技术设计

## 决策记录

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 模板组织 | 按 UI 库分目录 | 避免单文件 if/else 爆炸，提高可维护性 |
| 模块模式 | IUiLibraryProvider 接口 | 遵循开闭原则，新增 UI 库无需修改核心代码 |
| 预设存储 | 文件系统 JSON | 简单、可版本控制、无需数据库 |
| 架构模块 | 单一 ArchitectureModule | 架构间互斥，无需多模块 |
| ORM 模块 | 单一 OrmModule | ORM 间互斥，无需多模块 |

---

## 数据结构

### 枚举定义

```csharp
// Contracts/Enums/ArchitectureStyle.cs
public enum ArchitectureStyle
{
    Simple,
    CleanArchitecture,
    VerticalSlice,
    ModularMonolith
}

// Contracts/Enums/OrmProvider.cs
public enum OrmProvider
{
    SqlSugar,
    EFCore,
    Dapper,
    FreeSql
}

// Contracts/Enums/UiLibrary.cs
public enum UiLibrary
{
    ElementPlus,
    AntDesignVue,
    NaiveUI,
    TailwindHeadless,
    ShadcnVue,
    MateChat
}
```

### 请求 DTO 扩展

```csharp
// BackendOptions 扩展
public sealed record BackendOptions
{
    // 现有字段...

    [JsonPropertyName("architecture")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ArchitectureStyle Architecture { get; init; } = ArchitectureStyle.Simple;

    [JsonPropertyName("orm")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrmProvider Orm { get; init; } = OrmProvider.SqlSugar;
}

// FrontendOptions 扩展
public sealed record FrontendOptions
{
    // 现有字段...

    [JsonPropertyName("uiLibrary")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UiLibrary UiLibrary { get; init; } = UiLibrary.ElementPlus;
}
```

### 预设 DTO

```csharp
// Contracts/Requests/PresetConfig.cs
public sealed record PresetConfig
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("config")]
    public required GenerateScaffoldRequest Config { get; init; }
}
```

---

## 模块设计

### IUiLibraryProvider 接口

```csharp
public interface IUiLibraryProvider
{
    UiLibrary Library { get; }
    IEnumerable<PackageReference> GetNpmPackages();
    string GetMainTsTemplatePath();
    IEnumerable<string> GetAdditionalTemplates();
}
```

### ArchitectureModule

```csharp
public sealed class ArchitectureModule : IScaffoldModule
{
    public string Name => "Architecture";
    public int Order => 5; // 在 CoreModule 之后

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct)
    {
        var style = request.Backend.Architecture;
        // 根据架构类型添加对应目录结构模板
    }
}
```

### OrmModule

```csharp
public sealed class OrmModule : IScaffoldModule
{
    public string Name => "Orm";
    public int Order => 10;

    public bool IsEnabled(GenerateScaffoldRequest request) => true;

    public Task ContributeAsync(ScaffoldPlan plan, GenerateScaffoldRequest request, CancellationToken ct)
    {
        var orm = request.Backend.Orm;
        // 根据 ORM 类型添加对应 Setup 文件
    }
}
```

---

## 模板目录结构

```
templates/
├── backend/
│   ├── architecture/
│   │   ├── simple/
│   │   ├── clean/
│   │   ├── vertical-slice/
│   │   └── modular-monolith/
│   └── orm/
│       ├── sqlsugar/
│       ├── efcore/
│       ├── dapper/
│       └── freesql/
└── frontend/
    └── ui/
        ├── element-plus/
        ├── antd/
        ├── naive-ui/
        ├── tailwind/
        ├── shadcn-vue/
        └── matechat/
```

---

## API 设计

### 预设 API

| 端点 | 方法 | 描述 |
|------|------|------|
| `/api/v1/presets` | GET | 获取所有预设列表 |
| `/api/v1/presets/{name}` | GET | 获取单个预设 |
| `/api/v1/presets` | POST | 创建自定义预设 |
| `/api/v1/presets/{name}` | DELETE | 删除自定义预设 |

### 响应格式

```json
{
  "presets": [
    {
      "name": "enterprise",
      "description": "企业标准模版",
      "isBuiltin": true,
      "config": { ... }
    }
  ]
}
```

---

## 前端配置器变更

### ScaffoldConfig 类型扩展

```typescript
interface BackendConfig {
  // 现有字段...
  architecture: 'Simple' | 'CleanArchitecture' | 'VerticalSlice' | 'ModularMonolith';
  orm: 'SqlSugar' | 'EFCore' | 'Dapper' | 'FreeSql';
}

interface FrontendConfig {
  // 现有字段...
  uiLibrary: 'ElementPlus' | 'AntDesignVue' | 'NaiveUI' | 'TailwindHeadless' | 'ShadcnVue' | 'MateChat';
}
```

### 新增组件

- `ArchitectureSelector.vue` - 架构选择卡片
- `OrmSelector.vue` - ORM 选择卡片
- `UiLibrarySelector.vue` - UI 库选择卡片
- `PresetSelector.vue` - 预设选择/保存
