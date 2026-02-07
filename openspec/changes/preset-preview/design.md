# 技术设计

## 架构概览

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend (Vue 3)                        │
├─────────────────────────────────────────────────────────────┤
│  PresetSelector.vue  │  FileTreeView.vue  │  CodePreview.vue │
│         ↓                    ↓                    ↑          │
│  ┌─────────────────────────────────────────────────────────┐│
│  │                   config.ts (Pinia Store)               ││
│  │  - presets[], selectedPresetId, selectedFile            ││
│  │  - previewContent, previewLoading                       ││
│  └─────────────────────────────────────────────────────────┘│
│                              ↓                               │
│  ┌─────────────────────────────────────────────────────────┐│
│  │                   api/generator.ts                       ││
│  │  - getPresets()                                          ││
│  │  - previewFile(config, outputPath)                       ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Backend (.NET)                          │
├─────────────────────────────────────────────────────────────┤
│  GET /v1/scaffolds/presets                                   │
│  POST /v1/scaffolds/preview-file                             │
│         ↓                                                    │
│  ┌─────────────────────────────────────────────────────────┐│
│  │  PresetService          │  PreviewService               ││
│  │  - GetPresetsAsync()    │  - PreviewFileAsync()         ││
│  │  - BuiltInPresets       │  - LanguageMapper             ││
│  └─────────────────────────────────────────────────────────┘│
│                              ↓                               │
│  ┌─────────────────────────────────────────────────────────┐│
│  │  ScaffoldPlanBuilder  →  ScribanTemplateRenderer        ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

---

## 后端设计

### 目录结构新增

```
src/apps/api/
├── ScaffoldGenerator.Contracts/
│   ├── Presets/
│   │   ├── ScaffoldPresetDto.cs
│   │   └── ScaffoldPresetsResponse.cs
│   └── Preview/
│       ├── PreviewFileRequest.cs
│       ├── PreviewFileResponse.cs
│       └── PreviewFileRequestValidator.cs
├── ScaffoldGenerator.Application/
│   ├── Presets/
│   │   ├── IPresetService.cs
│   │   ├── PresetService.cs
│   │   └── BuiltInPresets.cs
│   └── Preview/
│       ├── IPreviewService.cs
│       ├── PreviewService.cs
│       └── LanguageMapper.cs
└── ScaffoldGenerator.Api/
    └── Endpoints/
        ├── PresetsEndpoints.cs
        └── PreviewEndpoints.cs
```

### 语言映射规则

```csharp
public static class LanguageMapper
{
    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        [".cs"] = "csharp",
        [".ts"] = "typescript",
        [".vue"] = "vue",
        [".json"] = "json",
        [".xml"] = "xml",
        [".csproj"] = "xml",
        [".html"] = "html",
        [".css"] = "css",
        [".scss"] = "scss",
        [".md"] = "markdown"
    };

    public static string GetLanguage(string filePath)
        => ExtensionMap.TryGetValue(Path.GetExtension(filePath).ToLowerInvariant(), out var lang)
            ? lang
            : "plaintext";
}
```

### 内置预设定义

```csharp
public static class BuiltInPresets
{
    public static IReadOnlyList<ScaffoldPresetDto> All => new[]
    {
        new ScaffoldPresetDto(
            Id: "minimal",
            Name: "Minimal",
            Description: "最小化配置，仅包含核心功能",
            IsDefault: false,
            Tags: ["lightweight", "quick-start"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new() { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new() { Database = DatabaseProvider.SQLite, Cache = CacheProvider.None, Swagger = true, JwtAuth = false },
                Frontend = new() { RouterMode = RouterMode.Hash, MockData = false }
            }
        ),
        new ScaffoldPresetDto(
            Id: "standard",
            Name: "Standard",
            Description: "标准配置，适合大多数项目",
            IsDefault: true,
            Tags: ["recommended"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new() { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new() { Database = DatabaseProvider.SQLite, Cache = CacheProvider.MemoryCache, Swagger = true, JwtAuth = true },
                Frontend = new() { RouterMode = RouterMode.Hash, MockData = false }
            }
        ),
        new ScaffoldPresetDto(
            Id: "enterprise",
            Name: "Enterprise",
            Description: "企业级配置，包含完整功能",
            IsDefault: false,
            Tags: ["full-featured", "production"],
            Config: new GenerateScaffoldRequest
            {
                Basic = new() { ProjectName = "MyApp", Namespace = "MyApp" },
                Backend = new() { Database = DatabaseProvider.MySQL, Cache = CacheProvider.Redis, Swagger = true, JwtAuth = true },
                Frontend = new() { RouterMode = RouterMode.History, MockData = false }
            }
        )
    };
}
```

---

## 前端设计

### 目录结构新增

```
src/apps/web-configurator/src/
├── components/
│   ├── PresetSelector.vue      # 预设选择器
│   └── CodePreview.vue         # 代码预览面板
├── composables/
│   └── useShiki.ts             # Shiki 高亮 hook
└── types/
    └── index.ts                # 扩展类型定义
```

### Store 扩展

```typescript
// stores/config.ts
export const useConfigStore = defineStore('config', () => {
  // 现有状态...

  // 新增状态
  const presets = ref<ScaffoldPreset[]>([])
  const selectedPresetId = ref<string | null>(null)
  const selectedFile = ref<FileTreeNode | null>(null)
  const previewContent = ref<PreviewFileResponse | null>(null)
  const previewLoading = ref(false)

  // 新增 Actions
  async function fetchPresets() {
    presets.value = await getPresets()
  }

  function applyPreset(presetId: string) {
    const preset = presets.value.find(p => p.id === presetId)
    if (preset) {
      selectedPresetId.value = presetId
      config.value = flattenConfig(preset.config)
    }
  }

  function selectFile(node: FileTreeNode) {
    if (!node.isDirectory) {
      selectedFile.value = node
      fetchPreview()
    }
  }

  async function fetchPreview() {
    if (!selectedFile.value) return
    previewLoading.value = true
    try {
      previewContent.value = await previewFile(config.value, selectedFile.value.path)
    } finally {
      previewLoading.value = false
    }
  }

  return {
    // 现有导出...
    presets, selectedPresetId, selectedFile, previewContent, previewLoading,
    fetchPresets, applyPreset, selectFile, fetchPreview
  }
})
```

### Shiki 集成

```typescript
// composables/useShiki.ts
import { ref, shallowRef } from 'vue'
import type { BundledLanguage, Highlighter } from 'shiki'

const highlighter = shallowRef<Highlighter | null>(null)
const loading = ref(false)

export function useShiki() {
  async function ensureLoaded() {
    if (highlighter.value) return highlighter.value
    if (loading.value) {
      // 等待加载完成
      await new Promise(resolve => {
        const check = setInterval(() => {
          if (highlighter.value) {
            clearInterval(check)
            resolve(highlighter.value)
          }
        }, 50)
      })
      return highlighter.value!
    }

    loading.value = true
    const { createHighlighter } = await import('shiki')
    highlighter.value = await createHighlighter({
      themes: ['github-light', 'github-dark'],
      langs: ['csharp', 'typescript', 'vue', 'json', 'html', 'xml']
    })
    loading.value = false
    return highlighter.value
  }

  async function highlight(code: string, language: string) {
    const hl = await ensureLoaded()
    return hl.codeToHtml(code, {
      lang: language as BundledLanguage,
      theme: 'github-light'
    })
  }

  return { highlight, loading }
}
```

---

## 风险缓解

| 风险 | 级别 | 缓解措施 |
|------|------|----------|
| 路径穿越攻击 | 高 | Validator 拒绝 `../`、绝对路径、反斜杠 |
| 预设与枚举漂移 | 高 | 启动时 ValidateAll()，失败则阻止启动 |
| 实时预览性能 | 中 | 前端防抖 300ms + CancellationToken |
| Shiki 包体积 | 中 | 动态 import + 仅加载必要语言 |
| 预览内容不一致 | 中 | 复用 ScaffoldPlanBuilder，保证逻辑一致 |
