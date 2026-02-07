# 状态管理

> 本项目的状态管理方式。

---

## 概述

本项目使用 **Pinia** 的 **Setup Store** 模式（Composition API 风格，非 Options API）。

**核心原则**：使用 `storeToRefs()` 解构响应式状态，直接解构方法。

---

## 状态分类

| 类别 | 位置 | 示例 |
|------|------|------|
| **局部状态** | 组件内 `ref()` | 表单输入值、UI 开关 |
| **共享状态** | Pinia store | 用户配置、应用设置 |
| **服务器状态** | Composable + API | 获取的数据、缓存响应 |
| **URL 状态** | Vue Router | 当前路由、查询参数 |

---

## Pinia Store 模式

**使用 Setup Store 语法**（非 Options API）：

```typescript
// src/stores/config.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ScaffoldConfig } from '@/types'

export const useConfigStore = defineStore('config', () => {
  // 状态
  const config = ref<ScaffoldConfig>({
    projectName: 'MyProject',
    namespace: 'MyProject',
    database: 'SQLite',
    cache: 'None',
    enableSwagger: true
  })
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters（计算属性）
  const isValid = computed(() => {
    return config.value.projectName.length > 0
  })

  // Actions（函数）
  function updateConfig(partial: Partial<ScaffoldConfig>) {
    config.value = { ...config.value, ...partial }
  }

  function reset() {
    config.value = {
      projectName: 'MyProject',
      namespace: 'MyProject',
      database: 'SQLite',
      cache: 'None',
      enableSwagger: true
    }
  }

  return {
    // 状态
    config,
    loading,
    error,
    // Getters
    isValid,
    // Actions
    updateConfig,
    reset
  }
})
```

---

## 消费 Store

### 正确模式

```typescript
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()

// ✅ 响应式状态：使用 storeToRefs
const { config, loading, isValid } = storeToRefs(store)

// ✅ 方法：直接解构
const { updateConfig, reset } = store
```

### 在模板中

```vue
<template>
  <el-input v-model="config.projectName" />
  <el-button :loading="loading" @click="reset">重置</el-button>
</template>
```

---

## 何时使用全局状态

| 使用全局状态 | 使用局部状态 |
|-------------|-------------|
| 多个组件需要相同数据 | 只有一个组件使用 |
| 状态需跨路由持久化 | 导航时重置 |
| 状态影响全局行为 | 仅 UI 状态（弹窗、悬停） |
| API 数据需要缓存 | 组件局部输入 |

---

## 服务器状态模式

API 获取的数据，结合 Pinia 和 composables：

```typescript
// Store 持有状态
export const useDataStore = defineStore('data', () => {
  const items = ref<Item[]>([])
  const loading = ref(false)

  async function fetchItems() {
    loading.value = true
    try {
      items.value = await api.getItems()
    } finally {
      loading.value = false
    }
  }

  return { items, loading, fetchItems }
})

// Composable 提供便捷访问
export function useItems() {
  const store = useDataStore()
  const { items, loading } = storeToRefs(store)

  onMounted(() => {
    if (items.value.length === 0) {
      store.fetchItems()
    }
  })

  return { items, loading, refresh: store.fetchItems }
}
```

---

## 派生状态

使用 `computed` 定义派生值：

```typescript
export const useConfigStore = defineStore('config', () => {
  const config = ref<ScaffoldConfig>({...})

  // 派生状态
  const hasDatabase = computed(() => config.value.database !== 'None')
  const hasCache = computed(() => config.value.cache !== 'None')
  const summary = computed(() => ({
    features: [
      hasDatabase.value && 'Database',
      hasCache.value && 'Caching'
    ].filter(Boolean)
  }))

  return { config, hasDatabase, hasCache, summary }
})
```

---

## Store 命名约定

| 元素 | 约定 | 示例 |
|------|------|------|
| **文件名** | 小写名词 | `config.ts`、`user.ts` |
| **Store ID** | 小写，与文件名匹配 | `'config'`、`'user'` |
| **导出名** | `use` + PascalCase + `Store` | `useConfigStore` |

---

## 常见错误

### 1. 直接解构（丢失响应性）

```typescript
// ❌ 错误：非响应式
const { config } = useConfigStore()

// ✅ 正确：保持响应性
const { config } = storeToRefs(useConfigStore())
```

### 2. 使用 Options API 风格

```typescript
// ❌ 错误：Options API 风格
export const useStore = defineStore('store', {
  state: () => ({ count: 0 }),
  actions: {
    increment() { this.count++ }
  }
})

// ✅ 正确：Setup Store 风格
export const useStore = defineStore('store', () => {
  const count = ref(0)
  function increment() { count.value++ }
  return { count, increment }
})
```

### 3. 在 actions 外部修改状态

```typescript
// ❌ 错误：组件内直接修改
store.config.projectName = 'New Name'

// ✅ 正确：使用 action
store.updateConfig({ projectName: 'New Name' })
```

### 4. 存储派生数据

```typescript
// ❌ 错误：存储计算值
const store = defineStore('store', () => {
  const items = ref([])
  const count = ref(0)  // 冗余！

  function addItem(item) {
    items.value.push(item)
    count.value = items.value.length  // 手动同步
  }
})

// ✅ 正确：使用 computed
const store = defineStore('store', () => {
  const items = ref([])
  const count = computed(() => items.value.length)
})
```

### 5. 过度使用全局状态

```typescript
// ❌ 错误：弹窗状态不需要全局
const useModalStore = defineStore('modal', () => {
  const isOpen = ref(false)
})

// ✅ 正确：UI 相关用局部状态
const isModalOpen = ref(false)
```

### 6. vee-validate 与 Pinia 数据流断裂

**场景**：使用 `vee-validate` 的 `useField` 时，表单值变化不会自动同步到 Pinia store。

```typescript
// ❌ 错误：useField 与 store 数据流断裂
const { value: projectName } = useField<string>('projectName')
// 用户修改 → vee-validate 更新 → store 不知道 → 依赖 store 的功能失效

// ✅ 正确：添加 watch 实时同步到 store
import { watch } from 'vue'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()
const { value: projectName } = useField<string>('projectName')

watch(projectName, (newVal) => {
  store.updateConfig({ projectName: newVal || '' })
})
```

**适用场景**：表单值变化需要触发其他响应式逻辑（预览刷新、联动计算等）

---

## computed + watch 联动模式

当 `computed` 派生状态变化需要触发副作用时，需要配合 `watch`：

```typescript
export const useConfigStore = defineStore('config', () => {
  const config = ref<Config>({...})
  const selectedFile = ref<FileNode | null>(null)

  // computed 负责派生状态
  const fileTree = computed(() => buildTree(config.value))

  // watch 负责副作用：当 config 变化时，更新关联状态并触发请求
  watch(config, () => {
    if (selectedFile.value) {
      // 同步更新关联状态（路径可能变了）
      const newNode = findInTree(fileTree.value, selectedFile.value.name)
      if (newNode) {
        selectedFile.value = newNode
        fetchPreview()  // 触发副作用
      } else {
        selectedFile.value = null  // 文件不存在了
      }
    }
  }, { deep: true })

  return { config, fileTree, selectedFile }
})
```

**原则**：
- `computed` = 纯派生，无副作用
- `watch` = 副作用触发器（API 调用、状态同步）
