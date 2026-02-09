<template>
  <el-form-item label="UI 组件库">
    <el-radio-group v-model="uiLibrary" class="card-radio-group">
      <el-radio-button
        v-for="item in uiLibraryOptions"
        :key="item.value"
        :value="item.value"
        class="card-radio"
      >
        <div class="card-content">
          <div class="card-title">{{ item.label }}</div>
          <div class="card-desc">{{ item.description }}</div>
        </div>
      </el-radio-button>
    </el-radio-group>
  </el-form-item>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useField } from 'vee-validate'
import { useConfigStore } from '@/stores/config'
import type { UiLibrary } from '@/types'

const store = useConfigStore()

const uiLibraryOptions = [
  { value: 'ElementPlus', label: 'Element Plus', description: 'Vue 3 主流组件库' },
  { value: 'AntDesignVue', label: 'Ant Design Vue', description: '企业级 UI 设计' },
  { value: 'NaiveUI', label: 'Naive UI', description: '轻量现代风格' },
  { value: 'TailwindHeadless', label: 'Tailwind + Headless', description: '原子化 CSS' },
  { value: 'ShadcnVue', label: 'shadcn-vue', description: '可定制组件集' },
  { value: 'MateChat', label: 'MateChat', description: 'AI 对话专用 UI' }
]

const { value: uiLibrary } = useField<UiLibrary>('uiLibrary')

watch(uiLibrary, (val) => {
  store.updateConfig({ uiLibrary: val })
})
</script>

<style scoped>
.card-radio-group {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  width: 100%;
}

.card-radio {
  margin: 0 !important;
  height: auto !important;
}

.card-radio :deep(.el-radio-button__inner) {
  width: 100%;
  padding: 12px;
  border-radius: 8px !important;
  border: 1px solid #e4e7ed !important;
  text-align: left;
  white-space: normal;
  line-height: 1.4;
}

.card-radio.is-active :deep(.el-radio-button__inner) {
  border-color: #409eff !important;
  background: #ecf5ff;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.card-title {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}

.card-desc {
  font-size: 12px;
  color: #909399;
}
</style>
