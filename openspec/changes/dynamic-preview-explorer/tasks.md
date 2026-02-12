# Tasks: Dynamic Preview Explorer

## Phase 1: 后端 DTO 和接口

### 1.1 创建 FileTreeNodeDto

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Preview/FileTreeNodeDto.cs`

**实现要点**:
- record 类型，属性: Name, Path, IsDirectory, Children?
- JsonPropertyName 确保 camelCase 输出

**验收**: 编译通过

---

### 1.2 创建 PreviewTreeRequest/Response

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Preview/PreviewTreeRequest.cs`

**实现要点**:
- PreviewTreeRequest 包含 GenerateScaffoldRequest Config
- PreviewTreeResponse 包含 List<FileTreeNodeDto> Tree

**验收**: 编译通过

---

### 1.3 扩展 IPreviewService 接口

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Preview/IPreviewService.cs`

**实现要点**:
- 新增 GetPreviewTreeAsync(GenerateScaffoldRequest, CancellationToken)
- 返回 PreviewTreeResponse

**验收**: 编译通过

---

## Phase 2: 后端树构建逻辑

### 2.1 实现树构建算法

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Preview/PreviewService.cs`

**实现要点**:
- 新增 GetPreviewTreeAsync 方法
- 调用 BuildAsync 获取 ScaffoldPlan
- 实现 BuildFileTree 私有方法：Files → Trie → 递归树
- 排序：目录优先，同级按名称排序

**验收**: 单元测试通过

---

### 2.2 单元测试 - 树构建

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Preview/PreviewServiceTreeTests.cs`

**实现要点**:
- 测试空文件列表 → 空树
- 测试单层文件 → 扁平树
- 测试多层目录 → 嵌套树
- 测试排序：目录优先
- 测试路径正确性：child.path = parent.path + "/" + child.name

**验收**: 所有测试通过

---

## Phase 3: 后端 API 端点

### 3.1 添加 Preview Tree 端点

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/Endpoints/ScaffoldEndpoints.cs`

**实现要点**:
- POST /api/scaffold/preview/tree
- 复用 GenerateScaffoldValidator 验证
- 调用 PreviewService.GetPreviewTreeAsync
- 返回 200 + PreviewTreeResponse

**验收**: 手动测试 API 返回正确结构

---

### 3.2 集成测试 - Preview Tree API

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Integration/PreviewTreeEndpointTests.cs`

**实现要点**:
- 测试 200：有效配置返回树
- 测试 400：空配置
- 测试树结构符合 FileTreeNode 契约
- 测试不同架构返回不同目录结构

**验收**: 所有测试通过

---

## Phase 4: 前端 API 客户端

### 4.1 添加 getPreviewTree 函数

- [x] **文件**: `src/apps/web-configurator/src/api/generator.ts`

**实现要点**:
- 新增 getPreviewTree(config: ScaffoldConfig): Promise<{ tree: FileTreeNode[] }>
- 复用 buildGenerateRequest 构建请求体
- POST /api/scaffold/preview/tree

**验收**: TypeScript 编译通过

---

## Phase 5: 前端 Store 改造

### 5.1 重构 fileTree 为异步状态

- [x] **文件**: `src/apps/web-configurator/src/stores/config.ts`

**实现要点**:
- 删除 fileTree computed 和 getExtensionFiles 函数
- 新增 fileTree = ref<FileTreeNode[]>([])
- 新增 treeLoading = ref(false)
- 新增 fetchFileTree() action
- 新增 fetchFileTreeDebounced() 防抖封装（300ms）
- 修改 watch(config) 调用 fetchFileTreeDebounced

**验收**: 预览面板打开时显示后端返回的文件树

---

### 5.2 处理选中文件同步

- [x] **文件**: `src/apps/web-configurator/src/stores/config.ts`

**实现要点**:
- fetchFileTree 成功后，检查 selectedFile 是否在新树中存在
- 不存在则清除 selectedFile 和 previewContent
- 存在则保持选中状态

**验收**: 切换配置后选中状态正确处理

---

### 5.3 单元测试 - Store 改造

- [x] **文件**: `src/apps/web-configurator/tests/stores/config.spec.ts`

**实现要点**:
- 测试 fetchFileTree 更新 fileTree
- 测试防抖：快速调用只触发一次请求
- 测试选中文件不存在时清除

**验收**: 所有测试通过

---

## Phase 6: 前端 UI 更新

### 6.1 FileTreeView 加载状态

- [x] **文件**: `src/apps/web-configurator/src/components/FileTreeView.vue`

**实现要点**:
- 引入 treeLoading 状态
- 显示 loading 时展示骨架屏（已有）
- 确保 loading 逻辑与新的 treeLoading 状态关联

**验收**: 配置变化时显示加载状态

---

### 6.2 E2E 测试 - 动态预览

- [x] **文件**: `src/apps/web-configurator/e2e/dynamic-preview.spec.ts`

**实现要点**:
- 测试切换架构后文件树更新
- 测试切换 ORM 后显示对应 Setup 文件
- 测试切换 UI 库后显示对应配置文件
- 测试选择 NuGet 包后预览 .csproj 包含包引用

**验收**: E2E 测试通过

---

## Phase 7: 清理

### 7.1 删除废弃代码

- [x] **文件**: `src/apps/web-configurator/src/stores/config.ts`

**实现要点**:
- 确认 getExtensionFiles 已删除
- 确认硬编码的 fileTree computed 已删除
- 清理未使用的导入

**验收**: Lint 通过，无未使用代码警告

---

## 进度统计

| Phase | 总任务 | 已完成 | 进度 |
|-------|--------|--------|------|
| Phase 1: 后端 DTO | 3 | 3 | 100% |
| Phase 2: 树构建 | 2 | 2 | 100% |
| Phase 3: API 端点 | 2 | 2 | 100% |
| Phase 4: 前端 API | 1 | 1 | 100% |
| Phase 5: Store 改造 | 3 | 3 | 100% |
| Phase 6: UI 更新 | 2 | 2 | 100% |
| Phase 7: 清理 | 1 | 1 | 100% |
| **总计** | **14** | **14** | **100%** |
