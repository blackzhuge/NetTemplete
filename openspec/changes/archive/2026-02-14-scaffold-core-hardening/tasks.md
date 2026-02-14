# Scaffold Core Hardening - Tasks

## Phase 1: 后端模板修复（REQ-1 + REQ-2）

### 1.1 修复 Program.cs.sbn 命名空间引用

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/templates/backend/Program.cs.sbn`

**实现要点**:

- 在第 1 行 `using {{ namespace }}.Api.Extensions;` 之后添加条件 using
- 条件：`orm == "EFCore" || orm == "Dapper" || orm == "FreeSql"`
- 添加 `using {{ namespace }}.Api.Setup;`
- SqlSugar 时不添加（保持现状）

**验收**: 生成的 Program.cs 在非 SqlSugar ORM 时包含 `using xxx.Api.Setup;`

---

### 1.2 补全 Dapper 驱动包 - Api.csproj.sbn

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/templates/backend/Api.csproj.sbn`

**实现要点**:

- 在 Dapper 分支（`{{~ else if orm == "Dapper" ~}}`）中，`Dapper` 包之后添加数据库驱动包
- SQLite → `Microsoft.Data.Sqlite` 9.0.0
- MySQL → `MySqlConnector` 2.4.0
- SQLServer → `Microsoft.Data.SqlClient` 5.2.2
- 使用与 EFCore/FreeSql 相同的 `{{~ if db_type == "xxx" ~}}` 条件模式

**验收**: 生成的 .csproj 在 Dapper 组合时包含对应驱动包

---

### 1.3 补全 Dapper 驱动包 - OrmModule.cs

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/OrmModule.cs`

**实现要点**:

- 在 `AddDapperFiles` 方法中，`plan.AddNugetPackage(new PackageReference("Dapper", "2.1.35"));` 之后添加 switch
- 按 `request.Backend.Database` 添加驱动包（与 1.2 版本一致）
- 模式参照 `AddFreeSqlFiles` 中的 switch 结构

**验收**: OrmModule 在 Dapper 组合时注册对应驱动包到 plan

---

### 1.4 后端模板修复 - 单元测试

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/OrmModuleTests.cs`（新增或扩展）

**实现要点**:

- 测试 Dapper + SQLite/MySQL/SQLServer 三种组合，验证 plan.NugetPackages 包含对应驱动包
- 测试 EFCore/Dapper/FreeSql 组合生成的 Program.cs 包含 `Api.Setup` using
- 测试 SqlSugar 组合生成的 Program.cs 不包含 `Api.Setup` using

**验收**: `dotnet test` 新增测试全部通过

---

### 1.5 后端模板修复 - 集成测试

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Api/GenerateEndpointTests.cs`（扩展）

**实现要点**:

- 扩展现有 `PreviewFile_DapperSelection_UsesDapperWithoutSqlSugar` 测试
- 添加 Dapper + SQLite/MySQL/SQLServer 的 preview-file 测试，验证 .csproj 包含驱动包
- 添加 EFCore/FreeSql 的 Program.cs preview 测试，验证包含 `using xxx.Api.Setup`

**验收**: `dotnet test` 集成测试全部通过

---

## Phase 2: 前端测试路径修复（REQ-3）

### 2.1 修复 FrontendUnitTestModule 输出路径

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/FrontendUnitTestModule.cs`

**实现要点**:

- 第 22 行：`$"{projectName}.Web/vitest.config.ts"` → `$"src/{projectName}.Web/vitest.config.ts"`

**验收**: vitest.config.ts 输出路径以 `src/` 开头

---

### 2.2 修复 FrontendE2EModule 输出路径

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/FrontendE2EModule.cs`

**实现要点**:

- Playwright 分支：`$"{projectName}.Web/playwright.config.ts"` → `$"src/{projectName}.Web/playwright.config.ts"`
- Cypress 分支：`$"{projectName}.Web/cypress.config.ts"` → `$"src/{projectName}.Web/cypress.config.ts"`

**验收**: playwright.config.ts 和 cypress.config.ts 输出路径以 `src/` 开头

---

### 2.3 前端路径修复 - 单元测试

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendUnitTestModuleTests.cs`（扩展）
- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendE2EModuleTests.cs`（新增或扩展）

**实现要点**:

- 修改现有 FrontendUnitTestModuleTests 断言：验证 OutputPath 以 `src/` 开头
- 新增 FrontendE2EModule 测试：验证 Playwright/Cypress 配置文件路径以 `src/` 开头
- 验证路径与 FrontendModule 的 `src/{projectName}.Web/` 前缀一致

**验收**: `dotnet test` 路径相关测试全部通过

---

## Phase 3: 前端 lint 闭环（REQ-4）

### 3.1 修正 package.json.sbn lint 脚本

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/templates/frontend/package.json.sbn`

**实现要点**:

- 替换 `"lint": "eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --fix"` 为：
  - `"lint": "eslint ."`
  - `"lint:fix": "eslint . --fix"`

**验收**: 生成的 package.json lint 脚本无 `--ext` 标志，lint 和 lint:fix 分开

---

### 3.2 新增 eslint.config.js.sbn 模板

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/templates/frontend/eslint.config.js.sbn`（新增）

**实现要点**:

- 参照 `src/apps/web-configurator/eslint.config.js` 创建静态模板
- 包含：@eslint/js recommended、eslint-plugin-vue flat/essential、@vue/eslint-config-typescript、skip-formatting
- ignores: dist、coverage、node_modules
- 自定义规则：vue/multi-word-component-names off、@typescript-eslint/no-unused-vars error（忽略 _ 前缀）、no-console warn

**验收**: 模板语法正确，Scriban 渲染无错误

---

### 3.3 新增 .prettierrc.sbn 模板

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Api/templates/frontend/.prettierrc.sbn`（新增）

**实现要点**:

- 静态 JSON 配置：semi false、singleQuote true、trailingComma es5、printWidth 100、tabWidth 2

**验收**: 模板语法正确

---

### 3.4 FrontendModule 添加 eslint 依赖和模板注册

- [x] **文件**: `src/apps/api/ScaffoldGenerator.Application/Modules/FrontendModule.cs`

**实现要点**:

- 在 `ContributeAsync` 中添加 eslint/prettier devDependencies（通过 plan.AddNpmPackage）：
  - eslint ^9.17.0、@eslint/js ^9.17.0、eslint-plugin-vue ^9.32.0
  - @vue/eslint-config-typescript ^14.1.4、@vue/eslint-config-prettier ^10.1.0
  - typescript-eslint ^8.18.2、prettier ^3.4.0
- 添加模板文件注册：
  - `plan.AddTemplateFile("frontend/eslint.config.js.sbn", $"{webPath}/eslint.config.js", model)`
  - `plan.AddTemplateFile("frontend/.prettierrc.sbn", $"{webPath}/.prettierrc", model)`

**验收**: 生成项目包含 eslint.config.js 和 .prettierrc，devDependencies 包含所有 eslint 包

---

### 3.5 前端 lint 闭环 - 单元测试

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Modules/FrontendModuleTests.cs`（扩展）

**实现要点**:

- 验证 plan.NpmPackages 包含 eslint、@eslint/js、eslint-plugin-vue 等 devDependencies
- 验证 plan.Files 包含 eslint.config.js 和 .prettierrc
- 验证 eslint 包的 IsDevDependency 标志为 true

**验收**: `dotnet test` lint 相关测试全部通过

---

### 3.6 前端 lint 闭环 - 集成测试

- [x] **文件**: `src/tests/ScaffoldGenerator.Tests/Api/GenerateEndpointTests.cs`（扩展）

**实现要点**:

- 添加 preview-file 测试：验证生成的 package.json 包含 `"lint": "eslint ."` 且不包含 `--ext`
- 添加 preview-file 测试：验证生成的 eslint.config.js 内容正确
- 添加文件树测试：验证 eslint.config.js 和 .prettierrc 出现在文件树中

**验收**: `dotnet test` 集成测试全部通过

---

## Phase 4: 回归验证

### 4.1 全量测试通过

- [x] **命令**: `dotnet test src/tests/ScaffoldGenerator.Tests/ScaffoldGenerator.Tests.csproj`

**验收**: 所有测试（含新增）通过，零失败

---

### 4.2 SqlSugar 默认组合回归

- [x] **手动验证**: 使用默认配置（SqlSugar + SQLite）生成项目，`dotnet build` 通过（测试套件已覆盖）

**验收**: SqlSugar 组合无回归

---

## 进度统计

| Phase | 总任务 | 已完成 | 进度 |
| ----- | ------ | ------ | ---- |
| Phase 1: 后端模板修复 | 5 | 5 | 100% |
| Phase 2: 前端路径修复 | 3 | 3 | 100% |
| Phase 3: 前端 lint 闭环 | 6 | 6 | 100% |
| Phase 4: 回归验证 | 2 | 2 | 100% |
| **总计** | **16** | **16** | **100%** |
