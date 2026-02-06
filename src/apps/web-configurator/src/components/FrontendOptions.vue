<template>
  <el-card class="frontend-options">
    <template #header>
      <span class="card-title">前端配置</span>
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
import type { RouterMode } from '@/types'

const { value: routerMode } = useField<RouterMode>('routerMode')
const { value: enableMockData } = useField<boolean>('enableMockData')
</script>

<style scoped>
.frontend-options {
  margin-bottom: 16px;
}

.card-title {
  font-weight: 600;
}
</style>
