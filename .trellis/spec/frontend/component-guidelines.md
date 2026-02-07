# 组件规范

> 本项目的组件构建方式。

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
