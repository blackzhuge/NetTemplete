# Package Manager Integration - 技术设计

## 架构决策

### 决策记录

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 缓存策略 | IMemoryCache, TTL=5min | 单实例部署，避免 Redis 复杂度 |
| 版本格式 | 仅精确版本 (x.y.z) | 简化验证逻辑，避免范围解析 |
| 包源治理 | 完全自由 | 支持企业私有源 |
| 冲突处理 | 拒绝操作 | 明确错误，避免隐式覆盖 |
| HTTP 客户端 | IHttpClientFactory typed client | 统一超时、重试、日志 |

---

## 数据结构

### PackageInfo (DTO)

```csharp
public sealed record PackageInfo(
    string Name,
    string Version,
    string Description,
    string? IconUrl = null
);
```

### PackageReference (值对象)

```csharp
public sealed record PackageReference(
    string Name,
    string Version,
    string? Source = null
);
```

### BackendOptions 扩展

```csharp
public sealed record BackendOptions
{
    // ... 现有字段 ...

    [JsonPropertyName("nugetPackages")]
    public List<PackageReference> NugetPackages { get; init; } = [];
}
```

### FrontendOptions 扩展

```csharp
public sealed record FrontendOptions
{
    // ... 现有字段 ...

    [JsonPropertyName("npmPackages")]
    public List<PackageReference> NpmPackages { get; init; } = [];
}
```

---

## API 设计

### 端点

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/packages/nuget/search` | 搜索 NuGet 包 |
| GET | `/api/v1/packages/npm/search` | 搜索 npm 包 |
| GET | `/api/v1/packages/nuget/{id}/versions` | 获取 NuGet 包版本 |
| GET | `/api/v1/packages/npm/{name}/versions` | 获取 npm 包版本 |

### 请求参数

```
?q={query}&source={sourceUrl}&take={count}
```

### 响应格式

```json
{
  "items": [
    { "name": "Serilog", "version": "3.1.1", "description": "..." }
  ],
  "totalCount": 100
}
```

### 错误响应

```json
{
  "error": "UPSTREAM_TIMEOUT",
  "message": "包源响应超时，请稍后重试"
}
```

| 错误码 | HTTP 状态 | 说明 |
|--------|-----------|------|
| VALIDATION_ERROR | 400 | 参数格式错误 |
| CONFLICT | 409 | 包名冲突 |
| UPSTREAM_TIMEOUT | 504 | 上游超时 |
| UPSTREAM_ERROR | 502 | 上游错误 |

---

## 服务层设计

### 接口定义

```csharp
// Application 层
public interface IPackageSearchService
{
    Task<PackageSearchResult> SearchAsync(
        string query,
        string? source,
        int take,
        CancellationToken ct);

    Task<IReadOnlyList<string>> GetVersionsAsync(
        string packageId,
        string? source,
        CancellationToken ct);
}
```

### 实现类

```csharp
// Infrastructure 层
public class NuGetSearchService : IPackageSearchService { }
public class NpmSearchService : IPackageSearchService { }
```

### DI 注册

```csharp
services.AddHttpClient<NuGetSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

services.AddHttpClient<NpmSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

services.AddMemoryCache();
```

---

## 缓存策略

| 参数 | 值 |
|------|-----|
| TTL | 5 分钟 |
| Key 格式 | `{ecosystem}:{source}:{query}:{take}` |
| 最大条目 | 1000 |
| 空结果缓存 | 是 (TTL=1min) |

---

## 前端组件设计

### PackageSelector.vue

**Props**:
```typescript
interface Props {
  managerType: 'nuget' | 'npm'
  modelValue: PackageReference[]
  systemPackages?: string[]  // 用于冲突检测
}
```

**核心逻辑**:
1. 搜索防抖 (300ms)
2. 版本加载 (选择包后)
3. 冲突检测 (添加前)
4. 内联包源切换 (el-popover)

### Store 扩展

```typescript
// stores/config.ts
const nugetPackages = ref<PackageReference[]>([])
const npmPackages = ref<PackageReference[]>([])

// 冲突检测
const systemNugetPackages = computed(() => [...])
```

---

## 模板渲染

### Api.csproj.sbn

```xml
<ItemGroup>
  <!-- 系统包 -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  {{~ for pkg in nuget_packages ~}}
  <PackageReference Include="{{ pkg.name }}" Version="{{ pkg.version }}" />
  {{~ end ~}}
</ItemGroup>
```

### package.json.sbn

```json
{
  "dependencies": {
    "vue": "^3.4.0",
    {{~ for pkg in npm_packages ~}}
    "{{ pkg.name }}": "{{ pkg.version }}"{{ if !for.last }},{{ end }}
    {{~ end ~}}
  }
}
```

---

## 预置常用包

### NuGet 常用包

```csharp
public static readonly IReadOnlyList<PackageInfo> PopularNugetPackages = new[]
{
    new PackageInfo("Serilog", "3.1.1", "日志框架"),
    new PackageInfo("AutoMapper", "12.0.1", "对象映射"),
    new PackageInfo("FluentValidation", "11.9.0", "验证框架"),
    new PackageInfo("MediatR", "12.2.0", "中介者模式"),
    new PackageInfo("Polly", "8.2.0", "弹性策略")
};
```

### npm 常用包

```typescript
export const popularNpmPackages: PackageInfo[] = [
  { name: 'axios', version: '1.6.2', description: 'HTTP 客户端' },
  { name: 'dayjs', version: '1.11.10', description: '日期处理' },
  { name: 'lodash-es', version: '4.17.21', description: '工具库' },
  { name: '@vueuse/core', version: '10.7.0', description: 'Vue 组合式工具' }
]
```

---

## 预置包源

```json
{
  "nugetSources": [
    { "name": "nuget.org", "url": "https://api.nuget.org/v3/index.json", "isDefault": true }
  ],
  "npmSources": [
    { "name": "npmjs.org", "url": "https://registry.npmjs.org/", "isDefault": true },
    { "name": "淘宝镜像", "url": "https://registry.npmmirror.com/", "isDefault": false }
  ]
}
```
