# UI 布局重构 - 技术设计

## 架构概览

```text
┌─────────────────────────────────────────────────────────────┐
│                        HomePage.vue                          │
├─────────────────────────────┬───────────────────────────────┤
│     config-sidebar          │      PreviewDrawer (右侧)      │
│     (全宽配置区域)            │  ┌─────────────────────────┐  │
│                             │  │ Tab: Explorer │ Code    │  │
│  ┌─────────────────────┐    │  ├─────────────────────────┤  │
│  │ PresetSelector      │    │  │ FileTreeView            │  │
│  │ ConfigForm          │    │  │ 或 CodePreview          │  │
│  │  - BasicOptions     │    │  └─────────────────────────┘  │
│  │  - BackendOptions   │◄───┤                               │
│  │  - FrontendOptions  │    │  触发: 右上角 "预览" 按钮      │
│  └─────────────────────┘    │                               │
└─────────────────────────────┴───────────────────────────────┘
```

## 组件设计

### 1. PreviewDrawer.vue（新增）

```typescript
// Props
interface Props {
  modelValue: boolean  // v-model 控制显隐
}

// 内部状态
const activeTab = ref<'explorer' | 'code'>('explorer')

// 联动逻辑
watch(() => store.selectedFile, (file) => {
  if (file) activeTab.value = 'code'
})
```

**el-drawer 配置**:

```vue
<el-drawer
  v-model="visible"
  direction="rtl"
  size="50%"
  :with-header="false"
  class="preview-drawer dark-theme"
>
```

### 2. PackageSelectorModal.vue（新增）

```typescript
// Props
interface Props {
  visible: boolean
  managerType: 'nuget' | 'npm'
  selectedPackages: PackageReference[]
}

// Emits
const emit = defineEmits<{
  'update:visible': [value: boolean]
  'confirm': [packages: PackageReference[]]
}>()

// 内部状态
const searchQuery = ref('')
const sortBy = ref<'relevance' | 'downloads'>('relevance')
const tempSelected = ref<PackageReference[]>([])
```

**布局结构**:

```text
┌─────────────────────────────────────────┐
│ 搜索依赖包                          [X] │
├─────────────────────────────────────────┤
│ [搜索框____] [源选择▼] [排序: 下载量▼]  │
├─────────────────────────────────────────┤
│ ┌─────────────────────────────────────┐ │
│ │ 📦 lodash                    v4.17  │ │
│ │ 工具库...  ⬇️ 50M/周  📅 2024-01   │ │
│ │                      [选择版本▼]   │ │
│ ├─────────────────────────────────────┤ │
│ │ 📦 axios                     v1.6   │ │
│ │ HTTP客户端  ⬇️ 40M/周  📅 2024-02  │ │
│ └─────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│ 已选择: lodash@4.17.21, axios@1.6.0    │
│                    [取消] [确认添加]    │
└─────────────────────────────────────────┘
```

### 3. 后端 DTO 扩展

```csharp
// PackageInfo.cs
public record PackageInfo(
    string Name,
    string Version,
    string Description,
    string? IconUrl,
    long? DownloadCount,      // 新增
    DateTimeOffset? LastUpdated  // 新增
);
```

### 4. NuGet 服务修改

```csharp
// NuGetSearchService.cs - 解析响应
var items = response?.Data?.Select(p => new PackageInfo(
    p.Id ?? string.Empty,
    p.Version ?? string.Empty,
    p.Description ?? string.Empty,
    p.IconUrl,
    p.TotalDownloads,  // 新增
    null               // lastUpdated 暂不实现
)).ToList() ?? [];
```

### 5. npm 服务修改

```csharp
// NpmSearchService.cs - 解析响应
var items = response?.Objects?.Select(o => new PackageInfo(
    o.Package?.Name ?? string.Empty,
    o.Package?.Version ?? string.Empty,
    o.Package?.Description ?? string.Empty,
    null,
    o.Downloads?.Weekly,  // 新增
    o.Package?.Date       // 新增
)).ToList() ?? [];
```

## 状态管理

### ConfigStore 扩展

```typescript
// stores/config.ts
export const useConfigStore = defineStore('config', () => {
  // 新增 Drawer 状态
  const showPreviewDrawer = ref(false)

  function openPreview() {
    showPreviewDrawer.value = true
  }

  function closePreview() {
    showPreviewDrawer.value = false
  }

  return {
    showPreviewDrawer,
    openPreview,
    closePreview
  }
})
```

## 样式方案

### 暗色 Drawer

```css
.preview-drawer.dark-theme {
  --el-drawer-bg-color: #1e1e1e;
}

.preview-drawer .el-tabs__header {
  background: #252526;
  border-bottom: 1px solid #333;
}

.preview-drawer .el-tabs__item {
  color: #bbbbbb;
}

.preview-drawer .el-tabs__item.is-active {
  color: #ffffff;
}
```

### 弹窗列表项

```css
.package-item {
  display: grid;
  grid-template-columns: 1fr auto;
  padding: 12px 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.package-item .pkg-meta {
  display: flex;
  gap: 16px;
  color: var(--el-text-color-secondary);
  font-size: 12px;
}
```
