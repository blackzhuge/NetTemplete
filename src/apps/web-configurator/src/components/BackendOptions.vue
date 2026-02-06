<template>
  <el-card class="backend-options">
    <template #header>
      <span class="card-title">后端配置</span>
    </template>

    <el-form label-width="120px" label-position="right">
      <el-form-item label="数据库">
        <el-radio-group v-model="database">
          <el-radio value="SQLite">SQLite</el-radio>
          <el-radio value="MySQL">MySQL</el-radio>
          <el-radio value="SQLServer">SQL Server</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="缓存">
        <el-radio-group v-model="cache">
          <el-radio value="None">无</el-radio>
          <el-radio value="MemoryCache">内存缓存</el-radio>
          <el-radio value="Redis">Redis</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="功能模块">
        <el-checkbox v-model="enableJwtAuth">JWT 认证</el-checkbox>
        <el-checkbox v-model="enableSwagger">Swagger 文档</el-checkbox>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { useField } from 'vee-validate'
import type { DatabaseProvider, CacheProvider } from '@/types'

const { value: database } = useField<DatabaseProvider>('database')
const { value: cache } = useField<CacheProvider>('cache')
const { value: enableJwtAuth } = useField<boolean>('enableJwtAuth')
const { value: enableSwagger } = useField<boolean>('enableSwagger')
</script>

<style scoped>
.backend-options {
  margin-bottom: 16px;
}

.card-title {
  font-weight: 600;
}
</style>
