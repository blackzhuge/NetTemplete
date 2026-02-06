# 组件规范

> 本项目的组件构建方式。

---

## 概述

本项目统一使用 **Vue 3 Composition API** 的 `<script setup>` 语法。所有组件遵循一致的结构模式。

**技术栈**：
- Vue 3.4+（Composition API）
- TypeScript 5.3+（严格模式）
- Element Plus 2.5+（UI 库）
- Sass（CSS 预处理）

---

## 组件结构

**标准模板**（按此顺序）：

```vue
<template>
  <div class="component-name">
    <!-- 模板内容 -->
  </div>
</template>

<script setup lang="ts">
// 1. Vue 导入（已自动导入，通常不需要）
// 2. 第三方库导入
// 3. 本地组件导入
// 4. Store/composable 导入
// 5. Props/emits 定义
// 6. 响应式状态
// 7. 计算属性
// 8. 方法
// 9. 生命周期钩子
</script>

<style scoped>
.component-name {
  /* 样式 */
}
</style>
```

**示例**：`src/apps/web-configurator/src/components/ConfigForm.vue`

```vue
<template>
  <div class="config-form">
    <BasicOptions />
    <BackendOptions />
    <FrontendOptions />
    <div class="actions">
      <el-button type="primary" :loading="downloading" @click="generate">
        生成项目
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import BasicOptions from './BasicOptions.vue'
import BackendOptions from './BackendOptions.vue'
import FrontendOptions from './FrontendOptions.vue'
import { useGenerator } from '@/composables/useGenerator'

const { downloading, generate } = useGenerator()
</script>

<style scoped>
.config-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
</style>
```

---

## Props 约定

### 模式一：Store 连接组件（配置表单类）

编辑共享状态的组件，直接消费 store：

```vue
<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()
const { config } = storeToRefs(store)
</script>

<template>
  <el-input v-model="config.projectName" />
</template>
```

### 模式二：展示型组件（使用 props）

可复用 UI 组件，使用类型化 props：

```vue
<script setup lang="ts">
interface Props {
  title: string
  loading?: boolean
  items: string[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  select: [item: string]
  close: []
}>()
</script>
```

---

## 样式模式

| 规则 | 实现方式 |
|------|----------|
| **作用域样式** | 始终使用 `<style scoped>` |
| **CSS 预处理器** | 复杂样式使用 Sass |
| **类命名** | BEM 风格：`.component-name`，`.component-name__element` |
| **Element Plus** | 使用内置类，避免覆盖 |

```vue
<style scoped lang="scss">
.config-form {
  &__header {
    margin-bottom: 16px;
  }

  &__actions {
    display: flex;
    gap: 8px;
  }
}
</style>
```

---

## Element Plus 用法

| 模式 | 示例 |
|------|------|
| **表单输入** | `<el-input v-model="value" />` |
| **按钮** | `<el-button type="primary" @click="handler">` |
| **反馈** | `ElMessage.success('完成！')` |
| **加载状态** | `:loading="isLoading"` prop |

**导入说明**：Element Plus 组件通过 `unplugin-vue-components` 自动导入。

---

## 无障碍访问

| 要求 | 实现方式 |
|------|----------|
| **语义化 HTML** | 使用合适的元素（`button`、`form`、`nav`） |
| **Element Plus a11y** | 依赖内置 ARIA 属性 |
| **键盘导航** | 确保可点击元素可聚焦 |
| **加载状态** | 异步操作期间显示视觉反馈 |

---

## 常见错误

### 1. 直接解构 store 状态（丢失响应性）

```typescript
// ❌ 错误：丢失响应性
const { config } = useConfigStore()

// ✅ 正确：保持响应性
const { config } = storeToRefs(useConfigStore())
```

### 2. 组件内复杂逻辑

```vue
<!-- ❌ 错误：组件内 API 调用 -->
<script setup>
async function handleSubmit() {
  const res = await axios.post('/api/data', form)
  // 复杂处理...
}
</script>

<!-- ✅ 正确：抽取到 composable -->
<script setup>
import { useSubmitForm } from '@/composables/useSubmitForm'
const { submit, loading } = useSubmitForm()
</script>
```

### 3. 缺少类型注解

```typescript
// ❌ 错误：隐式 any
const data = ref({})

// ✅ 正确：显式类型
const data = ref<ConfigData>({})
```

### 4. 深层相对导入

```typescript
// ❌ 错误：脆弱的路径
import Foo from '../../../components/Foo.vue'

// ✅ 正确：路径别名
import Foo from '@/components/Foo.vue'
```

### 5. 手动导入 Vue API

```typescript
// ❌ 不必要：已自动导入
import { ref, computed } from 'vue'

// ✅ 正确：直接使用
const count = ref(0)
const doubled = computed(() => count.value * 2)
```
