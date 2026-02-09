<template>
  <el-form-item label="架构风格">
    <el-radio-group v-model="architecture" class="card-radio-group">
      <el-radio-button
        v-for="item in architectureOptions"
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
import type { ArchitectureStyle } from '@/types'

const store = useConfigStore()

const architectureOptions = [
  { value: 'Simple', label: 'Simple', description: '单项目结构，快速开发' },
  { value: 'CleanArchitecture', label: 'Clean Architecture', description: '四层分离，企业级' },
  { value: 'VerticalSlice', label: 'Vertical Slice', description: '按功能切片组织' },
  { value: 'ModularMonolith', label: 'Modular Monolith', description: '模块化单体架构' }
]

const { value: architecture } = useField<ArchitectureStyle>('architecture')

watch(architecture, (val) => {
  store.updateConfig({ architecture: val })
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
