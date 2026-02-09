# UI 布局重构 - 实施任务

## Phase 1: 后端 API 扩展

### 1.1 扩展 PackageInfo DTO

- [ ] **文件**: `src/apps/api/ScaffoldGenerator.Contracts/Packages/PackageInfo.cs`

**实现要点**:

- 添加 `long? DownloadCount` 字段
- 添加 `DateTimeOffset? LastUpdated` 字段
- 保持 record 类型，字段可空

**验收**: 编译通过，现有测试不受影响

### 1.2 更新 NuGet DTO 映射

- [ ] **文件**: `src/apps/api/ScaffoldGenerator.Infrastructure/Packages/NuGetSearchService.cs`

**实现要点**:

- 在内部 DTO `NuGetPackage` 添加 `TotalDownloads` 字段
- 映射 `DownloadCount = p.TotalDownloads`
- `LastUpdated` 暂设为 `null`（避免 N+1 请求）

**验收**: NuGet 搜索返回 downloadCount 字段

### 1.3 更新 npm DTO 映射

- [ ] **文件**: `src/apps/api/ScaffoldGenerator.Infrastructure/Packages/NpmSearchService.cs`

**实现要点**:

- 在内部 DTO 添加 `Downloads` 和 `Date` 字段
- 映射 `DownloadCount = o.Downloads?.Weekly`
- 映射 `LastUpdated = o.Package?.Date`

**验收**: npm 搜索返回 downloadCount 和 lastUpdated 字段

### 1.4 后端单元测试

- [ ] **文件**: `src/apps/api/ScaffoldGenerator.Tests/Packages/`

**实现要点**:

- 测试 NuGet 响应解析（含 totalDownloads）
- 测试 npm 响应解析（含 downloads.weekly, date）
- 测试字段缺失时的容错处理（返回 null）

**验收**: 所有测试通过

---

## Phase 2: 前端类型与 API 同步

### 2.1 扩展前端类型定义

- [ ] **文件**: `src/apps/web-configurator/src/types/packages.ts`

**实现要点**:

- `PackageInfo` 添加 `downloadCount?: number`
- `PackageInfo` 添加 `lastUpdated?: string`

**验收**: TypeScript 编译通过

### 2.2 更新 API 客户端

- [ ] **文件**: `src/apps/web-configurator/src/api/packages.ts`

**实现要点**:

- 确认响应类型自动包含新字段（无需修改代码）
- 添加排序参数支持（可选，如后端支持）

**验收**: API 调用返回完整字段

---

## Phase 3: PreviewDrawer 组件

### 3.1 创建 PreviewDrawer 组件

- [ ] **文件**: `src/apps/web-configurator/src/components/PreviewDrawer.vue`

**实现要点**:

- 使用 `el-drawer` direction="rtl" size="50%"
- 内部使用 `el-tabs` 包含 Explorer 和 Code 两个 Tab
- 暗色主题样式（背景 #1e1e1e）
- 监听 `store.selectedFile` 自动切换到 Code Tab

**验收**: Drawer 可正常打开/关闭，Tab 切换流畅

### 3.2 更新 ConfigStore

- [ ] **文件**: `src/apps/web-configurator/src/stores/config.ts`

**实现要点**:

- 添加 `showPreviewDrawer: ref(false)`
- 添加 `openPreview()` 和 `closePreview()` 方法

**验收**: Store 状态正确响应

### 3.3 重构 HomePage 布局

- [ ] **文件**: `src/apps/web-configurator/src/views/HomePage.vue`

**实现要点**:

- 移除固定的 `.ide-preview` 区域
- 配置侧边栏获得全宽（移除 width: 420px 限制）
- 添加"预览"按钮触发 Drawer
- 引入 PreviewDrawer 组件

**验收**: 配置区域全宽显示，预览按钮可用

### 3.4 Drawer 组件单元测试

- [ ] **文件**: `src/apps/web-configurator/src/components/__tests__/PreviewDrawer.spec.ts`

**实现要点**:

- 测试 Drawer 打开/关闭状态
- 测试 Tab 切换
- 测试文件选择后自动切换 Tab

**验收**: Vitest 测试通过

---

## Phase 4: PackageSelectorModal 组件

### 4.1 创建包搜索弹窗组件

- [ ] **文件**: `src/apps/web-configurator/src/components/PackageSelectorModal.vue`

**实现要点**:

- 使用 `el-dialog` width="700px"
- 顶部：搜索框 + 源选择 + 排序下拉
- 中间：搜索结果列表（展示 name, desc, downloads, lastUpdated）
- 底部：已选包展示 + 确认/取消按钮
- 内部状态管理临时选中的包
- 排序逻辑：按下载量降序排列

**验收**: 弹窗可正常打开，搜索和选择功能可用

### 4.2 重构 PackageSelector 组件

- [ ] **文件**: `src/apps/web-configurator/src/components/PackageSelector.vue`

**实现要点**:

- 移除行内搜索框和结果列表
- 改为"添加依赖"按钮 + 已选包 Tag 列表
- 点击按钮打开 PackageSelectorModal
- 接收 Modal 确认事件更新 modelValue

**验收**: 现有功能保持，交互改为弹窗模式

### 4.3 弹窗组件单元测试

- [ ] **文件**: `src/apps/web-configurator/src/components/__tests__/PackageSelectorModal.spec.ts`

**实现要点**:

- 测试搜索触发和结果展示
- 测试排序功能
- 测试版本选择
- 测试批量添加和确认

**验收**: Vitest 测试通过

---

## Phase 5: E2E 测试

### 5.1 Drawer 交互 E2E 测试

- [ ] **文件**: `src/apps/web-configurator/e2e/preview-drawer.spec.ts`

**实现要点**:

- 测试预览按钮点击打开 Drawer
- 测试文件树点击选中文件
- 测试代码预览展示
- 测试 Drawer 关闭

**验收**: Playwright 测试通过

### 5.2 包搜索弹窗 E2E 测试

- [ ] **文件**: `src/apps/web-configurator/e2e/package-modal.spec.ts`

**实现要点**:

- 测试添加依赖按钮打开弹窗
- 测试搜索和结果展示
- 测试排序切换
- 测试添加包并确认

**验收**: Playwright 测试通过

---

## 进度统计

| Phase | 总任务 | 已完成 | 进度 |
|-------|--------|--------|------|
| Phase 1: 后端 API | 4 | 0 | 0% |
| Phase 2: 前端类型 | 2 | 0 | 0% |
| Phase 3: PreviewDrawer | 4 | 0 | 0% |
| Phase 4: PackageSelectorModal | 3 | 0 | 0% |
| Phase 5: E2E 测试 | 2 | 0 | 0% |
| **总计** | **15** | **0** | **0%** |
