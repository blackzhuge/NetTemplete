# 组件规范

> 本项目的组件构建方式，支持多 UI 库。

---

## 支持的 UI 库

| UI 库 | 特点 | 导入方式 |
|-------|------|----------|
| Element Plus | 企业级、功能全面 | 全局注册 |
| Ant Design Vue | 阿里设计语言 | 按需导入 |
| Naive UI | TS 优先、高性能 | 按需导入 |
| Tailwind + Headless UI | 原子化 CSS | 按需导入 |
| shadcn-vue | Radix 移植 | 复制组件 |
| MateChat | AI 对话专用 | 全局注册 |

---

## UI 库无关原则

编写组件时，遵循以下原则确保可在不同 UI 库间迁移：

### 1. 逻辑与 UI 分离

```vue
<script setup lang="ts">
// ✅ 正确：业务逻辑放在 composable
import { useConfigForm } from '@/composables/useConfigForm'

const { config, validate, submit } = useConfigForm()
</script>

<template>
  <!-- UI 组件可替换 -->
  <el-form v-model="config">...</el-form>
</template>
```

### 2. 使用类型而非具体组件

```typescript
// ✅ 正确：定义业务类型
interface SelectOption {
  label: string
  value: string | number
}

// 组件只关心数据，不关心 UI 库
const options: SelectOption[] = [...]
```

### 3. 样式隔离

```vue
<style scoped>
/* ✅ 使用自定义类，避免覆盖 UI 库样式 */
.my-component {
  /* 自定义样式 */
}

/* ❌ 避免直接覆盖 UI 库类 */
/* .el-button { ... } */
</style>
```

---

## 组件结构

```vue
<template>
  <div class="component-name">
    <!-- 模板内容 -->
  </div>
</template>

<script setup lang="ts">
// 1. 第三方库导入
// 2. 本地组件导入
// 3. Store/composable 导入
// 4. Props/emits 定义
// 5. 响应式状态
// 6. 计算属性
// 7. 方法
// 8. 生命周期钩子
</script>

<style scoped>
.component-name {
  /* 样式 */
}
</style>
```

---

## Props 约定

### Store 连接组件

```vue
<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()
const { config } = storeToRefs(store)  // ✅ 保持响应性
</script>

<template>
  <el-input v-model="config.projectName" />
</template>
```

### 展示型组件

```vue
<script setup lang="ts">
interface Props {
  title: string
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  select: [item: string]
}>()
</script>
```

---

## 样式模式

| 规则 | 实现方式 |
|------|----------|
| 作用域 | 始终 `<style scoped>` |
| 类命名 | BEM：`.component-name__element` |
| Element Plus | 使用内置类，避免覆盖 |

---

## 常见错误

| 错误 | 正确做法 |
|------|----------|
| `const { config } = useConfigStore()` | 使用 `storeToRefs()` |
| 组件内 API 调用 | 抽取到 composable |
| `ref({})` 无类型 | `ref<Type>({})` |
| `../../../components/` | 使用 `@/components/` |
