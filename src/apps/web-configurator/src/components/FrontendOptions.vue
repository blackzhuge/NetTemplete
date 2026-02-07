<template>
  <div class="config-section">
    <div class="section-header">
      <el-icon class="header-icon"><Monitor /></el-icon>
      <span class="section-title">前端配置</span>
    </div>

    <el-form label-position="top">
      <el-form-item label="路由模式">
        <el-radio-group v-model="routerMode">
          <el-radio value="Hash">Hash 模式</el-radio>
          <el-radio value="History">History 模式</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="其他选项">
        <div class="checkbox-group">
          <el-checkbox v-model="enableMockData" border>启用 Mock 数据</el-checkbox>
        </div>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useField } from 'vee-validate'
import { Monitor } from '@element-plus/icons-vue'
import { useConfigStore } from '@/stores/config'
import type { RouterMode } from '@/types'

const store = useConfigStore()

const { value: routerMode } = useField<RouterMode>('routerMode')
const { value: enableMockData } = useField<boolean>('enableMockData')

// 实时同步到 store，触发预览刷新
watch([routerMode, enableMockData], ([mode, mock]) => {
  store.updateConfig({
    routerMode: mode,
    enableMockData: mock
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
  color: #e6a23c;
  font-size: 18px;
}

.section-title {
  font-weight: 600;
  font-size: 15px;
}

.checkbox-group {
  width: 100%;
}

.checkbox-group .el-checkbox {
  margin: 0;
  width: 100%;
}
</style>
