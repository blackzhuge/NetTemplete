<template>
  <div class="config-form">
    <Form
      ref="formRef"
      v-slot="{ meta, errors }"
      :validation-schema="validationSchema"
      :initial-values="store.config"
      @submit="onSubmit"
    >
      <BasicOptions />
      <el-divider border-style="dashed" />
      <BackendOptions />
      <el-divider border-style="dashed" />
      <FrontendOptions />

      <div class="actions">
        <el-button
          type="primary"
          size="large"
          class="generate-btn"
          :loading="downloading"
          :disabled="!meta.valid || downloading"
          native-type="submit"
        >
          <el-icon v-if="!downloading"><Download /></el-icon>
          <span class="btn-text">{{ downloading ? '正在生成...' : '生成项目' }}</span>
        </el-button>
      </div>

      <div v-if="Object.keys(errors).length > 0" class="form-errors">
        <el-alert type="error" :closable="false" show-icon>
          <template #title>
            <span>请修正以下错误:</span>
          </template>
          <template #default>
            <ul class="error-list">
              <li v-for="(error, field) in errors" :key="field">{{ error }}</li>
            </ul>
          </template>
        </el-alert>
      </div>
    </Form>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Form } from 'vee-validate'
import type { FormContext } from 'vee-validate'
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

const formRef = ref<FormContext<ScaffoldConfig> | null>(null)

// 监听 store.config 变化，同步更新表单值（预设切换时触发）
watch(
  () => store.config,
  (newConfig) => {
    if (formRef.value) {
      formRef.value.setValues(newConfig)
    }
  },
  { deep: true }
)

function onSubmit(values: Record<string, unknown>) {
  store.updateConfig(values as unknown as ScaffoldConfig)
  generate()
}
</script>

<style scoped>
.config-form {
  display: flex;
  flex-direction: column;
}

.actions {
  padding: 16px 0;
  margin-top: 16px;
}

.generate-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  font-weight: 600;
  letter-spacing: 0.5px;
  border-radius: 8px;
  box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2);
}

.btn-text {
  margin-left: 8px;
}

.form-errors {
  margin-top: 16px;
}

.error-list {
  margin: 4px 0 0 0;
  padding-left: 16px;
  line-height: 1.4;
}
</style>
