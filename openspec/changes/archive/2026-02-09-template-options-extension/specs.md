# Template Options Extension - 需求规格

## 1. 架构模式选择

### REQ-1.1 架构枚举定义

**Given** 用户访问配置器
**When** 选择后端架构模式
**Then** 可选：Simple, CleanArchitecture, VerticalSlice, ModularMonolith

**约束**:
- 默认值: Simple
- 每种架构生成不同目录结构

### REQ-1.2 架构模板生成

**Given** 用户选择 CleanArchitecture
**When** 生成项目
**Then** 目录结构为: `src/{Project}.Api/`, `src/{Project}.Application/`, `src/{Project}.Domain/`, `src/{Project}.Infrastructure/`

**Given** 用户选择 VerticalSlice
**When** 生成项目
**Then** 目录结构为: `src/{Project}.Api/Features/{Feature}/`

**Given** 用户选择 ModularMonolith
**When** 生成项目
**Then** 目录结构为: `src/Modules/{Module}/`

---

## 2. ORM 选择

### REQ-2.1 ORM 枚举定义

**Given** 用户访问配置器
**When** 选择 ORM
**Then** 可选：SqlSugar, EFCore, Dapper, FreeSql

**约束**:
- 默认值: SqlSugar
- 每种 ORM 生成对应 Setup 文件

### REQ-2.2 ORM 模板生成

**Given** 用户选择 EFCore
**When** 生成项目
**Then** 生成 `DbContext.cs` + `EFCoreSetup.cs`，package 包含 `Microsoft.EntityFrameworkCore`

**Given** 用户选择 Dapper
**When** 生成项目
**Then** 生成 `DapperSetup.cs`，package 包含 `Dapper`

**Given** 用户选择 FreeSql
**When** 生成项目
**Then** 生成 `FreeSqlSetup.cs`，package 包含 `FreeSql`

---

## 3. UI 库选择

### REQ-3.1 UI 库枚举定义

**Given** 用户访问配置器
**When** 选择前端 UI 库
**Then** 可选：ElementPlus, AntDesignVue, NaiveUI, TailwindHeadless, ShadcnVue, MateChat

**约束**:
- 默认值: ElementPlus
- 每种 UI 库生成对应 main.ts 和样式导入

### REQ-3.2 UI 库模板生成

**Given** 用户选择 TailwindHeadless
**When** 生成项目
**Then** 额外生成 `tailwind.config.js` + `postcss.config.js`

**Given** 用户选择 MateChat
**When** 生成项目
**Then** package 包含 `@matechat/core` + `vue-devui` + `@devui-design/icons`

---

## 4. 预设模版库

### REQ-4.1 预设存储

**Given** 用户完成配置
**When** 点击"保存为预设"
**Then** 配置保存到 `presets/custom/{name}.json`

**约束**:
- 预设名称: 1-50 字符，字母数字下划线
- 预设格式: 符合 schema.json 验证

### REQ-4.2 预设加载

**Given** 用户打开配置器
**When** 选择已有预设
**Then** 配置表单填充预设值

### REQ-4.3 内置预设

**Given** 系统初始化
**When** 加载预设列表
**Then** 包含 3 个内置预设: enterprise, startup, ai-ready

---

## 5. 向后兼容

### REQ-5.1 默认值兼容

**Given** 旧版请求（无新字段）
**When** 调用生成 API
**Then** 使用默认值：Architecture=Simple, Orm=SqlSugar, UiLibrary=ElementPlus

---

## PBT 属性

### PROP-1 架构幂等性

**不变式**: 相同配置多次生成，目录结构一致
**伪造策略**: 随机配置，生成两次，比较目录树 hash

### PROP-2 ORM 互斥性

**不变式**: 只能选择一种 ORM，生成文件不冲突
**伪造策略**: 检查生成文件中 ORM 相关类只有一套

### PROP-3 UI 库依赖完整性

**不变式**: package.json 包含 UI 库所需全部依赖
**伪造策略**: 对每种 UI 库，执行 npm install 验证无缺失

### PROP-4 预设往返一致性

**不变式**: Save(config) → Load() → config' = config
**伪造策略**: 随机生成配置，保存后加载，深度比较

### PROP-5 向后兼容性

**不变式**: 旧版请求生成结果与升级前一致
**伪造策略**: 保存旧版生成结果，升级后重新生成，diff 比较
