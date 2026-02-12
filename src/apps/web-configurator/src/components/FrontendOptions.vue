<template>
  <div class="config-section">
    <div class="section-header">
      <el-icon class="header-icon"><Monitor /></el-icon>
      <span class="section-title">前端配置</span>
    </div>

    <el-form label-position="top">
      <UiLibrarySelector />

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

      <el-form-item label="单元测试框架">
        <el-select v-model="frontendUnitTestFramework" placeholder="选择单元测试框架">
          <el-option value="None" label="无" />
          <el-option value="Vitest" label="Vitest" />
        </el-select>
      </el-form-item>

      <el-form-item label="E2E 测试框架">
        <el-select v-model="frontendE2EFramework" placeholder="选择 E2E 测试框架">
          <el-option value="None" label="无" />
          <el-option value="Playwright" label="Playwright" />
          <el-option value="Cypress" label="Cypress" />
        </el-select>
      </el-form-item>

      <el-form-item label="npm 包">
        <PackageSelector
          manager-type="npm"
          v-model="store.npmPackages"
          :system-packages="store.systemNpmPackages"
        />
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useField } from 'vee-validate'
import { Monitor } from '@element-plus/icons-vue'
import { useConfigStore } from '@/stores/config'
import type { RouterMode, FrontendUnitTestFramework, FrontendE2EFramework } from '@/types'
import PackageSelector from './PackageSelector.vue'
import UiLibrarySelector from './UiLibrarySelector.vue'

const store = useConfigStore()

const { value: routerMode } = useField<RouterMode>('routerMode')
const { value: enableMockData } = useField<boolean>('enableMockData')
const { value: frontendUnitTestFramework } = useField<FrontendUnitTestFramework>('frontendUnitTestFramework')
const { value: frontendE2EFramework } = useField<FrontendE2EFramework>('frontendE2EFramework')

// 实时同步到 store，触发预览刷新
watch([routerMode, enableMockData, frontendUnitTestFramework, frontendE2EFramework], ([mode, mock, unitTest, e2e]) => {
  store.updateConfig({
    routerMode: mode,
    enableMockData: mock,
    frontendUnitTestFramework: unitTest,
    frontendE2EFramework: e2e
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
