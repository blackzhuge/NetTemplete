# Preset Preview - 需求规格

## Purpose

提供预设模板选择和文件实时预览功能，增强脚手架配置器的用户体验。

## Requirements

### REQ-001: 获取预设列表

**场景**: 用户加载配置器时获取可用预设

#### API: GET /v1/scaffolds/presets

**Response 200**:
```json
{
  "version": "1.0.0",
  "presets": [
    {
      "id": "minimal",
      "name": "Minimal",
      "description": "最小化配置，仅包含核心功能",
      "isDefault": false,
      "tags": ["lightweight"],
      "config": {
        "basic": { "projectName": "MyApp", "namespace": "MyApp" },
        "backend": { "database": "SQLite", "cache": "None", "swagger": true, "jwtAuth": false },
        "frontend": { "routerMode": "hash", "mockData": false }
      }
    }
  ]
}
```

**错误码**: 500 (预设读取失败)

---

### REQ-002: 文件内容预览

**场景**: 用户点击文件树节点时预览渲染后的文件内容

#### API: POST /v1/scaffolds/preview-file

**Request**:
```json
{
  "config": {
    "basic": { "projectName": "MyApp", "namespace": "MyApp" },
    "backend": { "database": "SQLite", "cache": "None", "swagger": true, "jwtAuth": true },
    "frontend": { "routerMode": "hash", "mockData": false }
  },
  "outputPath": "src/MyApp.Api/Program.cs"
}
```

**Response 200**:
```json
{
  "outputPath": "src/MyApp.Api/Program.cs",
  "content": "using Microsoft.AspNetCore.Builder;\n...",
  "language": "csharp",
  "isTemplate": true
}
```

**错误码**:
- 400: 请求格式无效
- 404: 该配置下找不到目标文件
- 422: 配置组合无效
- 500: 模板渲染失败

## Type Definitions

### 后端 DTO

```csharp
// ScaffoldPresetDto.cs
public sealed record ScaffoldPresetDto(
    string Id,
    string Name,
    string Description,
    bool IsDefault,
    IReadOnlyList<string> Tags,
    GenerateScaffoldRequest Config
);

// ScaffoldPresetsResponse.cs
public sealed record ScaffoldPresetsResponse(
    string Version,
    IReadOnlyList<ScaffoldPresetDto> Presets
);

// PreviewFileRequest.cs
public sealed record PreviewFileRequest(
    GenerateScaffoldRequest Config,
    string OutputPath
);

// PreviewFileResponse.cs
public sealed record PreviewFileResponse(
    string OutputPath,
    string Content,
    string Language,
    bool IsTemplate
);
```

### 前端类型

```typescript
// types/index.ts
export interface ScaffoldPreset {
  id: string
  name: string
  description: string
  isDefault: boolean
  tags: string[]
  config: ScaffoldConfig
}

export interface PreviewFileResponse {
  outputPath: string
  content: string
  language: string
  isTemplate: boolean
}
```

## PBT 属性 (Property-Based Testing)

| 属性 | 不变量 | 伪造策略 |
|------|--------|----------|
| 预设完整性 | 所有预设的 config 必须通过 GenerateScaffoldValidator | 生成随机预设配置，验证是否通过验证 |
| 预览一致性 | preview-file 返回内容 == generate-zip 中对应文件内容 | 生成 ZIP 后解压，对比预览内容 |
| 路径安全 | outputPath 不能包含 `../` 或绝对路径 | 注入恶意路径，验证返回 400 |
| 语言映射确定性 | 相同扩展名始终返回相同 language | 多次请求相同文件，验证 language 一致 |
| 幂等性 | 相同请求多次调用返回相同结果 | 发送相同请求 N 次，验证响应一致 |

## Success Criteria

- [ ] 预设 API 返回至少 3 个预设 (Minimal, Standard, Enterprise)
- [ ] 选择预设后配置表单自动填充
- [ ] 点击文件树中的文件节点，侧边栏显示代码预览
- [ ] 代码预览支持 C#/Vue/TypeScript/JSON 语法高亮
- [ ] 预览内容与实际生成结果一致
- [ ] 前端防抖处理 (300ms)，避免频繁 API 调用
- [ ] 路径安全验证通过，拒绝恶意路径
- [ ] 类型检查和 lint 通过

---

## REQ-003: 右侧预览 Drawer

**场景**: 用户需要预览生成的文件结构和代码

**Given**: 用户在配置页面
**When**: 点击"预览"按钮
**Then**: 右侧滑出 Drawer，包含文件树和代码预览 Tab

**约束**:
- Drawer 方向: `rtl`（从右侧滑出）
- Drawer 宽度: 50%（最小 400px）
- 默认激活 Tab: 文件树（Explorer）
- 点击文件自动切换到代码预览 Tab

### PBT 属性

**PROP: Drawer 状态幂等性**
- 不变量: 连续多次点击打开按钮，Drawer 状态保持一致
- 伪造策略: 快速点击 10 次，验证 `showPreviewDrawer` 最终为 true
