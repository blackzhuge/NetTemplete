<template>
  <el-card class="basic-options">
    <template #header>
      <div class="card-header">
        <el-icon class="header-icon"><Setting /></el-icon>
        <span class="card-title">基本配置</span>
      </div>
    </template>

    <el-form label-width="120px" label-position="right">
      <el-form-item label="项目名称" required :error="projectNameError">
        <el-input
          v-model="projectName"
          placeholder="输入项目名称"
          @input="syncNamespace"
          clearable
        />
      </el-form-item>

      <el-form-item label="命名空间" required :error="namespaceError">
        <el-input v-model="namespace" placeholder="输入命名空间" clearable />
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { useField } from 'vee-validate'
import { Setting } from '@element-plus/icons-vue'

const { value: projectName, errorMessage: projectNameError } = useField<string>('projectName')
const { value: namespace, errorMessage: namespaceError } = useField<string>('namespace')

function syncNamespace() {
  if (!namespace.value || namespace.value === projectName.value?.slice(0, -1)) {
    namespace.value = projectName.value || ''
  }
}
</script>

<style scoped>
.basic-options {
  margin-bottom: 16px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-icon {
  color: #409eff;
  font-size: 18px;
}

.card-title {
  font-weight: 600;
}
</style>
