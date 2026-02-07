<script setup lang="ts">
import { onMounted } from 'vue'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()

onMounted(() => {
  store.fetchPresets()
})

function handleChange(presetId: string) {
  store.applyPreset(presetId)
}
</script>

<template>
  <div class="preset-selector">
    <el-select
      :model-value="store.selectedPresetId"
      placeholder="选择预设配置"
      @change="handleChange"
      style="width: 100%"
    >
      <el-option
        v-for="preset in store.presets"
        :key="preset.id"
        :label="preset.name"
        :value="preset.id"
      >
        <div class="preset-option">
          <span class="preset-name">{{ preset.name }}</span>
          <span class="preset-desc">{{ preset.description }}</span>
        </div>
      </el-option>
    </el-select>
  </div>
</template>

<style scoped>
.preset-selector {
  margin-bottom: 16px;
}

.preset-option {
  display: flex;
  flex-direction: column;
  line-height: 1.4;
}

.preset-name {
  font-weight: 500;
}

.preset-desc {
  font-size: 12px;
  color: #909399;
}
</style>
