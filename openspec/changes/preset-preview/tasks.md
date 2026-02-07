# 脚手架配置器增强 - 实现任务

## Phase 1: 后端 API (Tasks 1-6)

### 1.1 预设 DTO
- [ ] 创建 `ScaffoldGenerator.Contracts/Presets/ScaffoldPresetDto.cs`
- [ ] 创建 `ScaffoldGenerator.Contracts/Presets/ScaffoldPresetsResponse.cs`

### 1.2 预览 DTO 和 Validator
- [ ] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileRequest.cs`
- [ ] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileResponse.cs`
- [ ] 创建 `ScaffoldGenerator.Contracts/Preview/PreviewFileRequestValidator.cs`
- [ ] Validator: OutputPath 非空，禁止 `../`、绝对路径、反斜杠

### 1.3 预设服务
- [ ] 创建 `ScaffoldGenerator.Application/Presets/IPresetService.cs`
- [ ] 创建 `ScaffoldGenerator.Application/Presets/PresetService.cs`
- [ ] 创建 `ScaffoldGenerator.Application/Presets/BuiltInPresets.cs`
- [ ] 定义 3 个内置预设: Minimal, Standard, Enterprise
- [ ] 启动时 ValidateAllPresets() 校验合法性

### 1.4 文件预览服务
- [ ] 创建 `ScaffoldGenerator.Application/Preview/IPreviewService.cs`
- [ ] 创建 `ScaffoldGenerator.Application/Preview/PreviewService.cs`
- [ ] 创建 `ScaffoldGenerator.Application/Preview/LanguageMapper.cs`
- [ ] 复用 ScaffoldPlanBuilder 构建计划
- [ ] 按 outputPath 匹配目标文件并渲染

### 1.5 API 端点
- [ ] 创建 `ScaffoldGenerator.Api/Endpoints/PresetsEndpoints.cs`
- [ ] 创建 `ScaffoldGenerator.Api/Endpoints/PreviewEndpoints.cs`
- [ ] 实现 `GET /v1/scaffolds/presets`
- [ ] 实现 `POST /v1/scaffolds/preview-file`
- [ ] 注册端点到 Program.cs

### 1.6 后端单元测试
- [ ] 创建 `ScaffoldGenerator.Tests/Application/PresetServiceTests.cs`
- [ ] 创建 `ScaffoldGenerator.Tests/Application/PreviewServiceTests.cs`
- [ ] 创建 `ScaffoldGenerator.Tests/Application/LanguageMapperTests.cs`
- [ ] 预设合法性测试 (所有预设通过验证)
- [ ] 预览渲染正确性测试
- [ ] 路径安全测试 (拒绝 `../`)
- [ ] 语言映射测试

---

## Phase 2: 前端实现 (Tasks 7-15)

### 2.1 依赖安装
- [ ] 安装 Shiki: `pnpm add shiki`

### 2.2 类型定义扩展
- [ ] 在 `types/index.ts` 添加 `ScaffoldPreset` 接口
- [ ] 在 `types/index.ts` 添加 `PreviewFileResponse` 接口

### 2.3 API 调用扩展
- [ ] 在 `api/generator.ts` 添加 `getPresets()` 函数
- [ ] 在 `api/generator.ts` 添加 `previewFile()` 函数

### 2.4 Pinia Store 扩展
- [ ] 添加状态: presets, selectedPresetId, selectedFile
- [ ] 添加状态: previewContent, previewLoading
- [ ] 添加 Action: fetchPresets()
- [ ] 添加 Action: applyPreset(presetId)
- [ ] 添加 Action: selectFile(node)
- [ ] 添加 Action: fetchPreview() (带防抖 300ms)

### 2.5 预设选择器组件
- [ ] 创建 `components/PresetSelector.vue`
- [ ] 使用 el-select 下拉选择
- [ ] 选择后调用 store.applyPreset()
- [ ] 显示预设名称和描述

### 2.6 Shiki Hook
- [ ] 创建 `composables/useShiki.ts`
- [ ] 延迟加载 Shiki (dynamic import)
- [ ] 缓存 highlighter 实例
- [ ] 实现 highlight(code, language) 函数
- [ ] 支持语言: csharp, typescript, vue, json, html, xml

### 2.7 代码预览组件
- [ ] 创建 `components/CodePreview.vue`
- [ ] 使用 useShiki() 高亮代码
- [ ] 显示行号
- [ ] 添加复制按钮
- [ ] 处理 Loading 状态

### 2.8 文件树交互增强
- [ ] 在 FileTreeView.vue 添加 `@node-click` 事件
- [ ] 仅文件节点可选中 (isDirectory === false)
- [ ] 高亮当前选中节点 (current-node-key)
- [ ] 调用 store.selectFile()

### 2.9 主页面布局更新
- [ ] 在 HomePage.vue 添加 PresetSelector 到配置表单顶部
- [ ] 添加 CodePreview 侧边栏面板
- [ ] 实现响应式三栏布局：配置 | 文件树 | 代码预览

---

## Phase 3: 集成验证 (Task 16)

### 3.1 E2E 测试
- [ ] 创建 `e2e/preset-preview.spec.ts`
- [ ] 测试: 选择预设 → 表单自动填充
- [ ] 测试: 点击文件 → 代码预览显示
- [ ] 测试: 修改配置 → 文件树更新
- [ ] 测试: 切换预设 → 预览内容更新

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
