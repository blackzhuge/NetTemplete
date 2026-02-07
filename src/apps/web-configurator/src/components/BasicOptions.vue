<template>
  <div class="config-section">
    <div class="section-header">
      <el-icon class="header-icon"><Setting /></el-icon>
      <span class="section-title">基本配置</span>
    </div>

    <el-form label-position="top">
      <el-form-item label="项目名称" required :error="projectNameError">
        <el-input
          v-model="projectName"
          placeholder="例如: MyProject"
          @input="syncNamespace"
          clearable
        />
      </el-form-item>

      <el-form-item label="命名空间" required :error="namespaceError">
        <el-input v-model="namespace" placeholder="例如: MyCompany.MyProject" clearable />
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useField } from 'vee-validate'
import { Setting } from '@element-plus/icons-vue'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()

const { value: projectName, errorMessage: projectNameError } = useField<string>('projectName')
const { value: namespace, errorMessage: namespaceError } = useField<string>('namespace')

function syncNamespace() {
  if (!namespace.value || namespace.value === projectName.value?.slice(0, -1)) {
    namespace.value = projectName.value || ''
  }
}

// 实时同步到 store，触发预览刷新
watch([projectName, namespace], ([newProjectName, newNamespace]) => {
  store.updateConfig({
    projectName: newProjectName || '',
    namespace: newNamespace || ''
  })
})
</script>

<style scoped>
.config-section {
  margin-bottom: 32px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 16px;
  color: #334155;
}

.header-icon {
  color: #409eff;
  font-size: 18px;
}

.section-title {
  font-weight: 600;
  font-size: 15px;
}
</style>
