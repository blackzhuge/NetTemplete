# Hook 规范

> 本项目的 composables（hooks）使用方式。

---

## 概述

在 Vue 3 中，"hooks" 称为 **composables** —— 封装和复用有状态逻辑的函数。本项目使用 composables 从组件中抽取业务逻辑。

**位置**：`src/composables/`

---

## Composable 结构

**标准模式**：

```typescript
import { ref, computed } from 'vue'

export function useFeatureName() {
  // 1. 响应式状态
  const loading = ref(false)
  const data = ref<DataType | null>(null)
  const error = ref<string | null>(null)

  // 2. 计算属性
  const isEmpty = computed(() => !data.value)

  // 3. 方法
  async function fetchData() {
    loading.value = true
    try {
      data.value = await api.getData()
    } catch (err: any) {
      error.value = err.message
    } finally {
      loading.value = false
    }
  }

  // 4. 返回对象
  return {
    loading,
    data,
    error,
    isEmpty,
    fetchData
  }
}
```

---

## 实际示例

**文件**：`src/apps/web-configurator/src/composables/useGenerator.ts`

```typescript
import { ref } from 'vue'
import { generateScaffold } from '@/api/generator'
import { useConfigStore } from '@/stores/config'
import { ElMessage } from 'element-plus'

export function useGenerator() {
  const store = useConfigStore()
  const downloading = ref(false)

  async function generate() {
    if (downloading.value) return
    downloading.value = true

    try {
      const blob = await generateScaffold(store.config)
      downloadBlob(blob, `${store.config.projectName}.zip`)
      ElMessage.success('项目生成成功！')
    } catch (err: any) {
      ElMessage.error('生成失败，请重试')
    } finally {
      downloading.value = false
    }
  }

  return {
    downloading,
    generate
  }
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
```

---

## 命名约定

| 规则 | 示例 |
|------|------|
| **前缀** | 始终以 `use` 开头 |
| **命名** | camelCase，描述性动词/名词 |
| **文件名** | 与函数名相同 |

```
useGenerator.ts      → export function useGenerator()
useFormValidation.ts → export function useFormValidation()
useFileTree.ts       → export function useFileTree()
```

---

## 数据获取模式

本项目使用 **Axios + composables** 进行数据获取：

```typescript
// 在 api/resource.ts 中
import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  timeout: 60000
})

export async function fetchResource(id: string): Promise<Resource> {
  const response = await api.get(`/resources/${id}`)
  return response.data
}

// 在 composables/useResource.ts 中
import { ref, onMounted } from 'vue'
import { fetchResource } from '@/api/resource'

export function useResource(id: string) {
  const data = ref<Resource | null>(null)
  const loading = ref(false)
  const error = ref<Error | null>(null)

  async function load() {
    loading.value = true
    error.value = null
    try {
      data.value = await fetchResource(id)
    } catch (e) {
      error.value = e as Error
    } finally {
      loading.value = false
    }
  }

  onMounted(load)

  return { data, loading, error, reload: load }
}
```

---

## Composable 分类

| 类别 | 用途 | 示例 |
|------|------|------|
| **数据获取** | 加载和管理服务器数据 | `useResource()` |
| **表单处理** | 验证、提交 | `useFormSubmit()` |
| **UI 状态** | 切换、弹窗、加载 | `useModal()` |
| **业务逻辑** | 领域特定操作 | `useGenerator()` |

---

## Composable 中访问 Store

Composables 可以访问 Pinia stores：

```typescript
export function useFeature() {
  const store = useConfigStore()

  // 访问 store 状态
  const config = store.config

  // 调用 store actions
  function updateSetting(key: string, value: any) {
    store.updateConfig({ [key]: value })
  }

  return { config, updateSetting }
}
```

---

## 常见错误

### 1. 缺少 loading/error 状态

```typescript
// ❌ 错误：没有 loading 状态
export function useFetch() {
  const data = ref(null)
  async function fetch() {
    data.value = await api.get()
  }
  return { data, fetch }
}

// ✅ 正确：完整的状态处理
export function useFetch() {
  const data = ref(null)
  const loading = ref(false)
  const error = ref(null)

  async function fetch() {
    loading.value = true
    error.value = null
    try {
      data.value = await api.get()
    } catch (e) {
      error.value = e
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, fetch }
}
```

### 2. 未防止重复请求

```typescript
// ❌ 错误：可能触发多次
async function submit() {
  await api.submit(data)
}

// ✅ 正确：防止双击
async function submit() {
  if (loading.value) return
  loading.value = true
  try {
    await api.submit(data)
  } finally {
    loading.value = false
  }
}
```

### 3. 暴露内部辅助函数

```typescript
// ❌ 错误：内部函数被暴露
return {
  data,
  fetch,
  formatData,    // 内部辅助，不应暴露
  parseResponse  // 内部辅助
}

// ✅ 正确：只暴露必要的
return { data, fetch }
```

### 4. 未显式类型化返回值

```typescript
// ❌ 错误：隐式类型
export function useData() {
  const data = ref({})
  return { data }
}

// ✅ 正确：显式泛型类型
export function useData() {
  const data = ref<DataType | null>(null)
  return { data }
}
```
