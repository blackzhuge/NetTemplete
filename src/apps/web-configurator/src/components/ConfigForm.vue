<template>
  <div class="config-form">
    <BasicOptions />
    <BackendOptions />
    <FrontendOptions />

    <div class="actions">
      <el-button
        type="primary"
        size="large"
        :loading="downloading"
        :disabled="!isValid"
        @click="generate"
      >
        <el-icon v-if="!downloading"><Download /></el-icon>
        {{ downloading ? '生成中...' : '生成项目' }}
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Download } from '@element-plus/icons-vue'
import BasicOptions from './BasicOptions.vue'
import BackendOptions from './BackendOptions.vue'
import FrontendOptions from './FrontendOptions.vue'
import { useConfigStore } from '@/stores/config'
import { useGenerator } from '@/composables/useGenerator'
import { storeToRefs } from 'pinia'

const store = useConfigStore()
const { config } = storeToRefs(store)
const { downloading, generate } = useGenerator()

const isValid = computed(() => {
  return config.value.projectName.trim().length > 0 &&
         config.value.namespace.trim().length > 0
})
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
</style>
