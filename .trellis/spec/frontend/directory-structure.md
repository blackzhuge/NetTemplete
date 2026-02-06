# 目录结构

> 前端代码的组织方式。

---

## 概述

本项目采用 **monorepo 结构**，前端应用位于 `src/apps/` 目录。主要前端应用是 `web-configurator`，使用 Vue 3 + TypeScript 构建。

**核心原则**：按**功能类型**组织（components/views/composables/stores），而非按业务领域。

---

## 目录布局

```
src/apps/web-configurator/
├── src/
│   ├── components/         # 可复用 UI 组件
│   │   ├── BasicOptions.vue
│   │   ├── BackendOptions.vue
│   │   ├── FrontendOptions.vue
│   │   ├── FileTreeView.vue
│   │   └── ConfigForm.vue
│   ├── views/              # 页面级组件（路由映射）
│   │   └── HomePage.vue
│   ├── composables/        # 组合式函数（Hooks）
│   │   └── useGenerator.ts
│   ├── stores/             # Pinia 状态存储
│   │   └── config.ts
│   ├── api/                # API 客户端和接口
│   │   └── generator.ts
│   ├── types/              # TypeScript 类型定义
│   │   └── index.ts
│   ├── router/             # Vue Router 配置
│   │   └── index.ts
│   ├── App.vue             # 根组件
│   ├── main.ts             # 应用入口
│   ├── auto-imports.d.ts   # 自动生成的导入类型
│   └── components.d.ts     # 自动生成的组件类型
├── vite.config.ts          # Vite 构建配置
├── tsconfig.json           # TypeScript 配置
└── package.json            # 依赖配置
```

---

## 模块组织

| 目录 | 用途 | 示例 |
|------|------|------|
| `components/` | 可复用 UI 组件，可跨页面使用 | `ConfigForm.vue`, `FileTreeView.vue` |
| `views/` | 页面级组件，映射到路由 | `HomePage.vue` |
| `composables/` | 状态逻辑抽取，业务逻辑封装 | `useGenerator.ts` |
| `stores/` | Pinia 全局状态管理 | `config.ts` |
| `api/` | HTTP 客户端设置和 API 接口函数 | `generator.ts` |
| `types/` | 共享 TypeScript 接口和类型 | `index.ts` |
| `router/` | 路由定义和守卫 | `index.ts` |

**决策标准**：
- **页面 vs 组件**：有路由的放 `views/`，可复用的放 `components/`
- **逻辑位置**：复杂逻辑 → `composables/`；简单逻辑 → 保留在组件内
- **共享状态**：跨组件状态 → `stores/`；局部状态 → 组件内 `ref()`

---

## 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| **组件文件** | PascalCase `.vue` | `BasicOptions.vue` |
| **Composable 文件** | camelCase + `use` 前缀 | `useGenerator.ts` |
| **Store 文件** | camelCase，描述性名词 | `config.ts` |
| **类型文件** | camelCase 或 `index.ts` | `types/index.ts` |
| **API 文件** | camelCase，资源名称 | `generator.ts` |

---

## 示例

**良好组织的模块**：`src/apps/web-configurator/src/`

- 组件扁平化（无深层嵌套）
- 清晰分离：UI（`components/`）vs 逻辑（`composables/`）vs 状态（`stores/`）
- 类型集中在 `types/index.ts`
- API 层独立在 `api/`

---

## 禁止模式

| 模式 | 原因 |
|------|------|
| 深层组件嵌套（`components/forms/inputs/text/...`） | 难以导航，应使用扁平结构 |
| 组件内混合关注点（API 调用 + 渲染） | 应抽取到 composables |
| 类型分散在各组件中 | 应集中在 `types/` 目录 |
| 组件内放工具函数 | 需要时应创建 `utils/` 目录 |
