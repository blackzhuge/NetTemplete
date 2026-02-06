# 质量规范

> 前端开发的代码质量标准。

---

## 概述

本项目通过以下方式保证质量：
- TypeScript 严格模式（编译时）
- ESLint（代码风格）
- Vite + vue-tsc（构建时类型检查）
- 自动导入插件（一致性）

---

## 质量检查命令

```bash
# 开发服务器
pnpm dev

# 类型检查 + 构建
pnpm build           # 运行：vue-tsc && vite build

# Lint 并修复
pnpm lint            # 运行：eslint . --ext .vue,.ts --fix
```

**构建失败条件**：
- 存在类型错误
- 未使用的变量/参数
- 严格模式下缺少类型注解

---

## 禁止模式

### 1. 使用 `any` 类型

```typescript
// ❌ 禁止
const data: any = response
function handle(x: any) {}

// ✅ 必须
const data: ApiResponse<User> = response
function handle(x: unknown) {}
```

### 2. 直接解构 store 状态

```typescript
// ❌ 禁止：丢失响应性
const { config } = useConfigStore()

// ✅ 必须
const { config } = storeToRefs(useConfigStore())
```

### 3. 组件内复杂逻辑

```typescript
// ❌ 禁止：组件内 API 调用
<script setup>
const response = await axios.get('/api/data')
const processed = response.data.map(...)
</script>

// ✅ 必须：抽取到 composable
<script setup>
import { useData } from '@/composables/useData'
const { data, loading } = useData()
</script>
```

### 4. 深层相对导入

```typescript
// ❌ 禁止
import Foo from '../../../components/Foo.vue'

// ✅ 必须
import Foo from '@/components/Foo.vue'
```

### 5. 手动导入 Vue API（已自动导入时）

```typescript
// ❌ 不必要（已自动导入）
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { defineStore } from 'pinia'

// ✅ 直接使用
const count = ref(0)
const router = useRouter()
```

### 6. 内联样式

```vue
<!-- ❌ 禁止 -->
<div style="color: red; margin: 10px;">

<!-- ✅ 必须：使用 scoped 样式 -->
<div class="error-text">

<style scoped>
.error-text {
  color: red;
  margin: 10px;
}
</style>
```

### 7. Options API

```typescript
// ❌ 禁止：Options API
export default {
  data() { return { count: 0 } },
  methods: { increment() { this.count++ } }
}

// ✅ 必须：Composition API
<script setup>
const count = ref(0)
const increment = () => count.value++
</script>
```

---

## 必需模式

### 1. Script setup 配合 TypeScript

```vue
<script setup lang="ts">
// 所有组件必须使用此模式
</script>
```

### 2. Scoped 样式

```vue
<style scoped>
/* 所有样式必须 scoped */
</style>
```

### 3. Refs 显式类型注解

```typescript
// 复杂类型，始终注解
const config = ref<ScaffoldConfig>({...})
const items = ref<Item[]>([])
const user = ref<User | null>(null)
```

### 4. 异步操作错误处理

```typescript
async function fetchData() {
  loading.value = true
  error.value = null
  try {
    data.value = await api.getData()
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : '未知错误'
  } finally {
    loading.value = false
  }
}
```

### 5. 加载状态反馈

```vue
<template>
  <el-button :loading="loading" @click="submit">
    {{ loading ? '提交中...' : '提交' }}
  </el-button>
</template>
```

---

## 导入顺序

`<script setup>` 中遵循此顺序：

```typescript
// 1. Vue API（通常已自动导入，往往不需要）
import { onMounted } from 'vue'

// 2. 第三方库
import { ElMessage } from 'element-plus'
import axios from 'axios'

// 3. 本地组件
import ConfigForm from '@/components/ConfigForm.vue'

// 4. Composables 和 stores
import { useGenerator } from '@/composables/useGenerator'
import { useConfigStore } from '@/stores/config'

// 5. 类型
import type { ScaffoldConfig } from '@/types'

// 6. 常量和工具
import { API_BASE_URL } from '@/constants'
```

---

## 测试要求

### 单元测试（推荐）

- 独立测试 composables
- 测试 store actions
- 测试工具函数

### 组件测试（关键 UI）

- 测试表单验证
- 测试用户交互
- 测试错误状态

### 无需测试

- 简单展示组件
- 直接的 prop 传递
- Element Plus 包装组件

---

## 代码审查清单

### 提交前

- [ ] `pnpm build` 通过（类型检查 + 构建）
- [ ] `pnpm lint` 通过（或问题已修复）
- [ ] 无 `console.log` 或调试语句
- [ ] 无 `any` 类型
- [ ] 所有 refs 有显式类型

### 审查者检查

- [ ] 组件使用 `<script setup lang="ts">`
- [ ] 样式是 scoped
- [ ] 复杂逻辑抽取到 composables
- [ ] Store 状态通过 `storeToRefs()` 访问
- [ ] 导入使用 `@/` 路径别名
- [ ] 错误状态已处理
- [ ] 加载状态已展示

---

## 性能指南

### 路由懒加载

```typescript
// router/index.ts
const routes = [
  {
    path: '/settings',
    component: () => import('@/views/SettingsPage.vue')
  }
]
```

### 避免 computed 副作用

```typescript
// ❌ 错误：computed 中的副作用
const items = computed(() => {
  console.log('计算中...')  // 副作用
  return data.value.filter(...)
})

// ✅ 正确：纯计算
const items = computed(() => data.value.filter(...))
```

### 用户输入防抖

```typescript
import { useDebounceFn } from '@vueuse/core'

const search = useDebounceFn((query: string) => {
  // API 调用
}, 300)
```

---

## 构建验证

提交前确保：

```bash
# 1. 类型检查通过
pnpm exec vue-tsc --noEmit

# 2. 构建成功
pnpm build

# 3. Lint 通过
pnpm lint
```

**任何命令失败，提交前必须修复。**
