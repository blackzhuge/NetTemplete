# Scaffold Core Hardening - Specs

## Requirements

### REQ-1: Non-SqlSugar ORM 命名空间可见性

生成的 Program.cs MUST 包含所有被引用的扩展方法所在命名空间的 using 声明。

#### Scenario: EFCore ORM selected

- **GIVEN** ORM = EFCore, Database = any
- **WHEN** 系统生成 Program.cs
- **THEN** 文件 MUST 包含 `using {{ namespace }}.Api.Setup;`
- **AND** `builder.Services.AddEFCore(builder.Configuration)` 编译通过

#### Scenario: Dapper ORM selected

- **GIVEN** ORM = Dapper, Database = any
- **WHEN** 系统生成 Program.cs
- **THEN** 文件 MUST 包含 `using {{ namespace }}.Api.Setup;`
- **AND** `builder.Services.AddDapper(builder.Configuration)` 编译通过

#### Scenario: FreeSql ORM selected

- **GIVEN** ORM = FreeSql, Database = any
- **WHEN** 系统生成 Program.cs
- **THEN** 文件 MUST 包含 `using {{ namespace }}.Api.Setup;`
- **AND** `builder.Services.AddFreeSql(builder.Configuration)` 编译通过

#### Scenario: SqlSugar ORM selected (regression guard)

- **GIVEN** ORM = SqlSugar
- **WHEN** 系统生成 Program.cs
- **THEN** 文件 MUST NOT 包含 `using {{ namespace }}.Api.Setup;`
- **AND** `using {{ namespace }}.Api.Extensions;` 保持不变

### REQ-2: Dapper 数据库驱动依赖补全

选择 Dapper ORM 时，系统 MUST 根据 Database 类型添加对应的 ADO.NET 驱动包。

#### Scenario: Dapper + SQLite

- **GIVEN** ORM = Dapper, Database = SQLite
- **WHEN** 系统生成 .csproj
- **THEN** MUST 包含 `<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />`

#### Scenario: Dapper + MySQL

- **GIVEN** ORM = Dapper, Database = MySQL
- **WHEN** 系统生成 .csproj
- **THEN** MUST 包含 `<PackageReference Include="MySqlConnector" Version="2.4.0" />`

#### Scenario: Dapper + SQLServer

- **GIVEN** ORM = Dapper, Database = SQLServer
- **WHEN** 系统生成 .csproj
- **THEN** MUST 包含 `<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />`

### REQ-3: 前端测试配置输出路径修正

前端测试模块生成的配置文件 MUST 与 `package.json` 位于同一目录（`src/{ProjectName}.Web/`）。

#### Scenario: Vitest config path

- **GIVEN** FrontendUnitTestFramework = Vitest
- **WHEN** 系统生成文件树
- **THEN** `vitest.config.ts` 输出路径 MUST 为 `src/{ProjectName}.Web/vitest.config.ts`

#### Scenario: Playwright config path

- **GIVEN** FrontendE2EFramework = Playwright
- **WHEN** 系统生成文件树
- **THEN** `playwright.config.ts` 输出路径 MUST 为 `src/{ProjectName}.Web/playwright.config.ts`

#### Scenario: Cypress config path

- **GIVEN** FrontendE2EFramework = Cypress
- **WHEN** 系统生成文件树
- **THEN** `cypress.config.ts` 输出路径 MUST 为 `src/{ProjectName}.Web/cypress.config.ts`

### REQ-4: 前端 lint 闭环

生成的前端项目 MUST 包含完整的 eslint 工具链，使 `pnpm lint` 可零错误执行。

#### Scenario: lint script syntax

- **WHEN** 系统生成 package.json
- **THEN** `scripts.lint` MUST 为 `"eslint ."`（无 `--ext` 标志）
- **AND** `scripts["lint:fix"]` MUST 为 `"eslint . --fix"`

#### Scenario: eslint config exists

- **WHEN** 系统生成前端项目
- **THEN** MUST 包含 `eslint.config.js` 文件
- **AND** 使用 ESLint 9.x flat config 格式

#### Scenario: eslint devDependencies

- **WHEN** 系统生成 package.json
- **THEN** devDependencies MUST 包含：eslint, @eslint/js, eslint-plugin-vue, @vue/eslint-config-typescript, @vue/eslint-config-prettier, typescript-eslint

#### Scenario: prettier config exists

- **WHEN** 系统生成前端项目
- **THEN** MUST 包含 `.prettierrc` 文件（因为引入了 @vue/eslint-config-prettier）

---

## PBT Properties

### P1: ORM-Namespace Consistency (不变式)

- **不变式**: 对于任意 ORM x Database 组合，生成的 Program.cs 中引用的所有扩展方法所在命名空间 MUST 有对应的 using 声明
- **伪造策略**: 枚举所有 4 ORM x 3 DB = 12 种组合，对每种组合检查 Program.cs 的 using 声明是否覆盖实际调用的扩展方法命名空间

### P2: Dapper Driver Completeness (不变式)

- **不变式**: 当 ORM = Dapper 时，.csproj 中的 PackageReference 集合 MUST 包含 DapperSetup.cs 中 using 声明对应的所有 NuGet 包
- **伪造策略**: 对 Dapper x {SQLite, MySQL, SQLServer} 3 种组合，提取 DapperSetup.cs 的 using 声明，验证每个 using 的包在 .csproj 中存在

### P3: Frontend Path Prefix Consistency (不变式)

- **不变式**: 所有前端模块（FrontendModule, FrontendUnitTestModule, FrontendE2EModule）生成的文件路径 MUST 共享相同的根前缀 `src/{ProjectName}.Web/`
- **伪造策略**: 收集所有前端模块的 OutputPath，验证每个路径都以 `src/{ProjectName}.Web/` 开头

### P4: Lint Toolchain Completeness (不变式)

- **不变式**: 生成的 package.json 中 scripts.lint 引用的工具（eslint）MUST 在 devDependencies 中声明，且对应的配置文件 MUST 存在于文件树中
- **伪造策略**: 解析 package.json scripts 中的命令名，验证每个命令对应的包在 devDependencies 中存在，且配置文件在文件树中存在
