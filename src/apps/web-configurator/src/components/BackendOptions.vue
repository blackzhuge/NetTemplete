<template>
  <div class="config-section">
    <div class="section-header">
      <el-icon class="header-icon"><Cpu /></el-icon>
      <span class="section-title">后端配置</span>
    </div>

    <el-form label-position="top">
      <ArchitectureSelector />

      <OrmSelector />

      <el-form-item label="数据库">
        <el-radio-group v-model="database" class="full-width-radio">
          <el-radio value="SQLite">SQLite</el-radio>
          <el-radio value="MySQL">MySQL</el-radio>
          <el-radio value="SQLServer">SQL Server</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="缓存">
        <el-radio-group v-model="cache" class="full-width-radio">
          <el-radio value="None">无</el-radio>
          <el-radio value="MemoryCache">内存缓存</el-radio>
          <el-radio value="Redis">Redis</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="功能模块">
        <div class="checkbox-group">
          <el-checkbox v-model="enableJwtAuth" border>JWT 认证</el-checkbox>
          <el-checkbox v-model="enableSwagger" border>Swagger 文档</el-checkbox>
        </div>
      </el-form-item>

      <el-form-item label="单元测试框架">
        <el-select v-model="backendUnitTestFramework" placeholder="选择单元测试框架">
          <el-option value="None" label="无" />
          <el-option value="xUnit" label="xUnit" />
          <el-option value="NUnit" label="NUnit" />
          <el-option value="MSTest" label="MSTest" />
        </el-select>
      </el-form-item>

      <el-form-item label="集成测试框架">
        <el-select v-model="backendIntegrationTestFramework" placeholder="选择集成测试框架">
          <el-option value="None" label="无" />
          <el-option value="xUnit" label="xUnit + WebApplicationFactory" />
        </el-select>
      </el-form-item>

      <el-form-item label="NuGet 包">
        <PackageSelector
          manager-type="nuget"
          v-model="store.nugetPackages"
          :system-packages="store.systemNugetPackages"
        />
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useField } from 'vee-validate'
import { Cpu } from '@element-plus/icons-vue'
import { useConfigStore } from '@/stores/config'
import type { DatabaseProvider, CacheProvider, BackendUnitTestFramework, BackendIntegrationTestFramework } from '@/types'
import PackageSelector from './PackageSelector.vue'
import ArchitectureSelector from './ArchitectureSelector.vue'
import OrmSelector from './OrmSelector.vue'

const store = useConfigStore()

const { value: database } = useField<DatabaseProvider>('database')
const { value: cache } = useField<CacheProvider>('cache')
const { value: enableJwtAuth } = useField<boolean>('enableJwtAuth')
const { value: enableSwagger } = useField<boolean>('enableSwagger')
const { value: backendUnitTestFramework } = useField<BackendUnitTestFramework>('backendUnitTestFramework')
const { value: backendIntegrationTestFramework } = useField<BackendIntegrationTestFramework>('backendIntegrationTestFramework')

// 实时同步到 store，触发预览刷新
watch([database, cache, enableJwtAuth, enableSwagger, backendUnitTestFramework, backendIntegrationTestFramework], ([db, c, jwt, swagger, unitTest, integrationTest]) => {
  store.updateConfig({
    database: db,
    cache: c,
    enableJwtAuth: jwt,
    enableSwagger: swagger,
    backendUnitTestFramework: unitTest,
    backendIntegrationTestFramework: integrationTest
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
  color: #67c23a;
  font-size: 18px;
}

.section-title {
  font-weight: 600;
  font-size: 15px;
}

.full-width-radio {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}

.checkbox-group .el-checkbox {
  margin: 0;
  width: 100%;
}
</style>
