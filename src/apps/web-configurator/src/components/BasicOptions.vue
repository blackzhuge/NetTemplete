<template>
  <el-card class="basic-options">
    <template #header>
      <span class="card-title">基本配置</span>
    </template>

    <el-form :model="config" label-width="120px" label-position="right">
      <el-form-item label="项目名称" required>
        <el-input
          v-model="config.projectName"
          placeholder="输入项目名称"
          @input="syncNamespace"
        />
      </el-form-item>

      <el-form-item label="命名空间" required>
        <el-input v-model="config.namespace" placeholder="输入命名空间" />
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { useConfigStore } from '@/stores/config'
import { storeToRefs } from 'pinia'

const store = useConfigStore()
const { config } = storeToRefs(store)

function syncNamespace() {
  if (config.value.namespace === '' || config.value.namespace === config.value.projectName.slice(0, -1)) {
    config.value.namespace = config.value.projectName
  }
}
</script>

<style scoped>
.basic-options {
  margin-bottom: 16px;
}

.card-title {
  font-weight: 600;
}
</style>
