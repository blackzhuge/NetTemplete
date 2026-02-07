# 脚手架配置器增强 - 实现任务

## Phase 1: 后端 API (Tasks 1-6) ✅ COMPLETED

### 1.1 预设 DTO
- [x] 创建 `ScaffoldGenerator.Contracts/Presets/ScaffoldPresetDto.cs`
- [x] 创建 `ScaffoldGenerator.Contracts/Presets/ScaffoldPresetsResponse.cs`

### 1.2 预览 DTO 和 Validator
- [x] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileRequest.cs`
- [x] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileResponse.cs`
- [x] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileRequestValidator.cs`
- [x] Validator: OutputPath 非空，禁止 `../`、绝对路径、反斜杠

### 1.3 预设服务
- [x] 创建 `ScaffoldGenerator.Application/Presets/IPresetService.cs`
- [x] 创建 `ScaffoldGenerator.Application/Presets/PresetService.cs`
- [x] 创建 `ScaffoldGenerator.Application/Presets/BuiltInPresets.cs`
- [x] 定义 3 个内置预设: Minimal, Standard, Enterprise
- [x] 启动时 ValidateAllPresets() 校验合法性

### 1.4 文件预览服务
- [x] 创建 `ScaffoldGenerator.Application/Preview/IPreviewService.cs`
- [x] 创建 `ScaffoldGenerator.Application/Preview/PreviewService.cs`
- [x] 创建 `ScaffoldGenerator.Application/Preview/LanguageMapper.cs`
- [x] 复用 ScaffoldPlanBuilder 构建计划
- [x] 按 outputPath 匹配目标文件并渲染

### 1.5 API 端点
- [x] 创建 `ScaffoldGenerator.Api/Endpoints/PresetsEndpoints.cs`
- [x] 创建 `ScaffoldGenerator.Api/Endpoints/PreviewEndpoints.cs`
- [x] 实现 `GET /v1/scaffolds/presets`
- [x] 实现 `POST /v1/scaffolds/preview-file`
- [x] 注册端点到 Program.cs

### 1.6 后端单元测试
- [x] 创建 `ScaffoldGenerator.Tests/Application/PresetServiceTests.cs`
- [x] 创建 `ScaffoldGenerator.Tests/Application/PreviewServiceTests.cs`
- [x] 创建 `ScaffoldGenerator.Tests/Application/LanguageMapperTests.cs`
- [x] 预设合法性测试 (所有预设通过验证)
- [x] 预览渲染正确性测试
- [x] 路径安全测试 (拒绝 `../`)
- [x] 语言映射测试

---

## Phase 2: 前端实现 (Tasks 7-15) ✅ COMPLETED

### 2.1 依赖安装
- [x] 安装 Shiki: `pnpm add shiki`

### 2.2 类型定义扩展
- [x] 在 `types/index.ts` 添加 `ScaffoldPreset` 接口
- [x] 在 `types/index.ts` 添加 `PreviewFileResponse` 接口

### 2.3 API 调用扩展
- [x] 在 `api/generator.ts` 添加 `getPresets()` 函数
- [x] 在 `api/generator.ts` 添加 `previewFile()` 函数

### 2.4 Pinia Store 扩展
- [x] 添加状态: presets, selectedPresetId, selectedFile
- [x] 添加状态: previewContent, previewLoading
- [x] 添加 Action: fetchPresets()
- [x] 添加 Action: applyPreset(presetId)
- [x] 添加 Action: selectFile(node)
- [x] 添加 Action: fetchPreview() (带防抖 300ms)

### 2.5 预设选择器组件
- [x] 创建 `components/PresetSelector.vue`
- [x] 使用 el-select 下拉选择
- [x] 选择后调用 store.applyPreset()
- [x] 显示预设名称和描述

### 2.6 Shiki Hook
- [x] 创建 `composables/useShiki.ts`
- [x] 延迟加载 Shiki (dynamic import)
- [x] 缓存 highlighter 实例
- [x] 实现 highlight(code, language) 函数
- [x] 支持语言: csharp, typescript, vue, json, html, xml

### 2.7 代码预览组件
- [x] 创建 `components/CodePreview.vue`
- [x] 使用 useShiki() 高亮代码
- [x] 显示行号
- [x] 添加复制按钮
- [x] 处理 Loading 状态

### 2.8 文件树交互增强
- [x] 在 FileTreeView.vue 添加 `@node-click` 事件
- [x] 仅文件节点可选中 (isDirectory === false)
- [x] 高亮当前选中节点 (current-node-key)
- [x] 调用 store.selectFile()

### 2.9 主页面布局更新
- [x] 在 HomePage.vue 添加 PresetSelector 到配置表单顶部
- [x] 添加 CodePreview 侧边栏面板
- [x] 实现响应式三栏布局：配置 | 文件树 | 代码预览

---

## Phase 3: 集成验证 (Task 16) ✅ COMPLETED

### 3.1 E2E 测试
- [x] 创建 `e2e/preset-preview.spec.ts`
- [x] 测试: 选择预设 → 表单自动填充
- [x] 测试: 点击文件 → 代码预览显示
- [x] 测试: 修改配置 → 文件树更新
- [x] 测试: 切换预设 → 预览内容更新

---

## Milestones

| Milestone | 交付物 | 依赖 |
|-----------|--------|------|
| M1 | 后端预设 API 可用 | Phase 1.1-1.3 |
| M2 | 后端预览 API 可用 | Phase 1.4-1.5 |
| M3 | 前端预设选择可工作 | Phase 2.1-2.5 |
| M4 | 前端代码预览可工作 | Phase 2.6-2.9 |
| M5 | 端到端流程完整 | Phase 3 |

---

## Dependencies

```
Phase 1.1-1.2 ── Phase 1.3-1.4 ── Phase 1.5 ── Phase 1.6
                                      │
                                      ▼
Phase 2.1-2.2 ── Phase 2.3-2.4 ── Phase 2.5-2.9 ── Phase 3
```

Phase 1 (后端) 和 Phase 2.1-2.4 (前端基础) 可并行开发。
