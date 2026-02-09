# Design: Dynamic Preview Explorer

## 决策记录

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 服务设计 | 扩展 PreviewService | 复用现有 DI 注册和 BuildAsync，最小改动 |
| API 路由 | POST /api/scaffold/preview/tree | 与现有 preview 语义一致 |
| 树构建算法 | Files → Trie → 递归树 | O(F*S) 复杂度，规模可控 |
| 前端状态 | fileTree 改为 ref | 支持异步更新 |
| 防抖策略 | 300ms | 与现有 previewDebounce 一致 |
| 缓存策略 | 前端 computed 自动缓存 | 用户决策 |
| 错误处理 | 不做特殊处理 | 用户决策 |

---

## 数据结构

### 后端 DTO

```csharp
// ScaffoldGenerator.Contracts/Preview/FileTreeNodeDto.cs
public sealed record FileTreeNodeDto(
    string Name,
    string Path,
    bool IsDirectory,
    List<FileTreeNodeDto>? Children = null
);

// ScaffoldGenerator.Contracts/Preview/PreviewTreeRequest.cs
public sealed record PreviewTreeRequest(
    GenerateScaffoldRequest Config
);

// ScaffoldGenerator.Contracts/Preview/PreviewTreeResponse.cs
public sealed record PreviewTreeResponse(
    List<FileTreeNodeDto> Tree
);
```

### 前端类型

```typescript
// 复用现有 FileTreeNode，无需修改
export interface FileTreeNode {
  name: string
  path: string
  isDirectory: boolean
  children?: FileTreeNode[]
}
```

---

## API 设计

### POST /api/scaffold/preview/tree

**请求体**：
```json
{
  "basic": { "projectName": "MyProject", "namespace": "MyProject" },
  "backend": { "architecture": "Simple", "orm": "SqlSugar", ... },
  "frontend": { "uiLibrary": "ElementPlus", ... }
}
```

**响应体**：
```json
{
  "tree": [
    {
      "name": "MyProject.sln",
      "path": "MyProject.sln",
      "isDirectory": false
    },
    {
      "name": "src",
      "path": "src",
      "isDirectory": true,
      "children": [...]
    }
  ]
}
```

**状态码**：
| 状态码 | 含义 |
|--------|------|
| 200 | 成功，返回树（可为空数组） |
| 400 | 请求格式错误/字段校验失败 |
| 422 | 配置组合无效 |

---

## 后端实现

### PreviewService 扩展

```csharp
// IPreviewService.cs 新增方法
Task<PreviewTreeResponse> GetPreviewTreeAsync(
    GenerateScaffoldRequest request,
    CancellationToken ct = default);

// PreviewService.cs 实现
public async Task<PreviewTreeResponse> GetPreviewTreeAsync(
    GenerateScaffoldRequest request,
    CancellationToken ct = default)
{
    var plan = await _planBuilder.BuildAsync(request, ct);
    var tree = BuildFileTree(plan.Files);
    return new PreviewTreeResponse(tree);
}

private List<FileTreeNodeDto> BuildFileTree(IReadOnlyList<ScaffoldFile> files)
{
    // 1. 构建路径 Trie
    // 2. 递归转换为 FileTreeNodeDto[]
    // 3. 排序：目录优先，同级按名称排序
}
```

### 树构建算法

```
输入: ["src/Api/Program.cs", "src/Api/appsettings.json", "src/Web/package.json"]

步骤1 - 构建 Trie:
  root
   └─ src
       ├─ Api
       │   ├─ Program.cs (file)
       │   └─ appsettings.json (file)
       └─ Web
           └─ package.json (file)

步骤2 - 转换为 FileTreeNode[]:
  [
    { name: "src", path: "src", isDirectory: true, children: [...] }
  ]
```

---

## 前端实现

### Store 改造 (config.ts)

```typescript
// 删除
const fileTree = computed<FileTreeNode[]>(() => { ... })
function getExtensionFiles(): FileTreeNode[] { ... }

// 新增
const fileTree = ref<FileTreeNode[]>([])
const treeLoading = ref(false)

async function fetchFileTree() {
  treeLoading.value = true
  try {
    const response = await getPreviewTree(config.value)
    fileTree.value = response.tree
  } catch {
    // 保持当前状态，不做特殊处理
  } finally {
    treeLoading.value = false
  }
}

// 防抖调用
let treeDebounceTimer: ReturnType<typeof setTimeout> | null = null

function fetchFileTreeDebounced() {
  if (treeDebounceTimer) clearTimeout(treeDebounceTimer)
  treeDebounceTimer = setTimeout(fetchFileTree, 300)
}

// watch 调整
watch(config, () => {
  fetchFileTreeDebounced()
  onConfigChange()
}, { deep: true })
```

### API 客户端 (generator.ts)

```typescript
export async function getPreviewTree(
  config: ScaffoldConfig
): Promise<{ tree: FileTreeNode[] }> {
  const request = buildGenerateRequest(config)
  const response = await api.post('/api/scaffold/preview/tree', request)
  return response.data
}
```

---

## 边界情况处理

| 场景 | 处理 |
|------|------|
| 空配置 | 返回 400 |
| 无效配置组合 | 返回 422 |
| 构建成功但 files 为空 | 返回 200 + 空数组 |
| 前端请求失败 | 保持当前文件树不变 |
| 选中文件在新树中不存在 | 清除选中状态 |
