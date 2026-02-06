<template>
  <el-card class="frontend-options">
    <template #header>
      <span class="card-title">前端配置</span>
    </template>

    <el-form :model="config" label-width="120px" label-position="right">
      <el-form-item label="路由模式">
        <el-radio-group v-model="config.routerMode">
          <el-radio value="Hash">Hash 模式</el-radio>
          <el-radio value="History">History 模式</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="其他选项">
        <el-checkbox v-model="config.enableMockData">启用 Mock 数据</el-checkbox>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { useConfigStore } from '@/stores/config'
import { storeToRefs } from 'pinia'

const store = useConfigStore()
const { config } = storeToRefs(store)
</script>

<style scoped>
.frontend-options {
  margin-bottom: 16px;
}

.card-title {
  font-weight: 600;
}
</style>
