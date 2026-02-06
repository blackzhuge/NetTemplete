<template>
  <div class="config-form">
    <Form
      v-slot="{ meta, errors }"
      :validation-schema="validationSchema"
      :initial-values="initialValues"
      @submit="onSubmit"
    >
      <BasicOptions />
      <BackendOptions />
      <FrontendOptions />

      <div class="actions">
        <el-button
          type="primary"
          size="large"
          :loading="downloading"
          :disabled="!meta.valid || downloading"
          native-type="submit"
        >
          <el-icon v-if="!downloading"><Download /></el-icon>
          {{ downloading ? '生成中...' : '生成项目' }}
        </el-button>
      </div>

      <div v-if="Object.keys(errors).length > 0" class="form-errors">
        <el-alert type="error" :closable="false">
          <ul>
            <li v-for="(error, field) in errors" :key="field">{{ error }}</li>
          </ul>
        </el-alert>
      </div>
    </Form>
  </div>
</template>

<script setup lang="ts">
import { Form } from 'vee-validate'
import { Download } from '@element-plus/icons-vue'
import BasicOptions from './BasicOptions.vue'
import BackendOptions from './BackendOptions.vue'
import FrontendOptions from './FrontendOptions.vue'
import { useConfigStore } from '@/stores/config'
import { useGenerator } from '@/composables/useGenerator'
import { validationSchema } from '@/schemas/config'
import type { ScaffoldConfig } from '@/types'

const store = useConfigStore()
const { downloading, generate } = useGenerator()

const initialValues: ScaffoldConfig = {
  projectName: 'MyProject',
  namespace: 'MyProject',
  database: 'SQLite',
  cache: 'None',
  enableSwagger: true,
  enableJwtAuth: true,
  routerMode: 'Hash',
  enableMockData: false
}

function onSubmit(values: ScaffoldConfig) {
  store.updateConfig(values)
  generate()
}
</script>

<style scoped>
.config-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.actions {
  display: flex;
  justify-content: center;
  padding: 16px 0;
}

.actions .el-button {
  min-width: 200px;
}

.form-errors {
  margin-top: 16px;
}

.form-errors ul {
  margin: 0;
  padding-left: 20px;
}
</style>
