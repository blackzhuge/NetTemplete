<template>
  <el-card class="frontend-options">
    <template #header>
      <div class="card-header">
        <el-icon class="header-icon"><Monitor /></el-icon>
        <span class="card-title">前端配置</span>
      </div>
    </template>

    <el-form label-width="120px" label-position="right">
      <el-form-item label="路由模式">
        <el-radio-group v-model="routerMode">
          <el-radio value="Hash">Hash 模式</el-radio>
          <el-radio value="History">History 模式</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="其他选项">
        <el-checkbox v-model="enableMockData">启用 Mock 数据</el-checkbox>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { useField } from 'vee-validate'
import { Monitor } from '@element-plus/icons-vue'
import type { RouterMode } from '@/types'

const { value: routerMode } = useField<RouterMode>('routerMode')
const { value: enableMockData } = useField<boolean>('enableMockData')
</script>

<style scoped>
.frontend-options {
  margin-bottom: 16px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-icon {
  color: #e6a23c;
  font-size: 18px;
}

.card-title {
  font-weight: 600;
}
</style>
