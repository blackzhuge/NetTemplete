<script setup lang="ts">
import { ref, computed } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import type { PackageReference, PackageManagerType } from '@/types/packages'
import PackageSelectorModal from '@/components/PackageSelectorModal.vue'

interface Props {
  managerType: PackageManagerType
  modelValue: PackageReference[]
  systemPackages?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  systemPackages: () => []
})

const emit = defineEmits<{
  'update:modelValue': [packages: PackageReference[]]
}>()

// 状态
const showModal = ref(false)

// 计算属性
const packages = computed(() => props.modelValue)

function handleOpenModal() {
  showModal.value = true
}

function handleConfirm(newPackages: PackageReference[]) {
  emit('update:modelValue', [...packages.value, ...newPackages])
}

function handleRemovePackage(packageName: string) {
  emit('update:modelValue', packages.value.filter(p => p.name !== packageName))
}
</script>

<template>
  <div class="package-selector">
    <!-- 添加按钮 -->
    <el-button type="primary" plain @click="handleOpenModal">
      <el-icon><Plus /></el-icon>
      添加{{ managerType === 'nuget' ? 'NuGet' : 'npm' }}依赖
    </el-button>

    <!-- 已选择的包 -->
    <div v-if="packages.length > 0" class="selected-packages">
      <el-tag
        v-for="pkg in packages"
        :key="pkg.name"
        closable
        @close="handleRemovePackage(pkg.name)"
      >
        {{ pkg.name }}@{{ pkg.version }}
      </el-tag>
    </div>

    <!-- 弹窗 -->
    <PackageSelectorModal
      v-model:visible="showModal"
      :manager-type="managerType"
      :existing-packages="packages"
      :system-packages="systemPackages"
      @confirm="handleConfirm"
    />
  </div>
</template>

<style scoped>
.package-selector {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.selected-packages {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>
