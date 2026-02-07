<script setup lang="ts">
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { PackageInfo, PackageReference, PackageManagerType, PackageSource } from '@/types/packages'
import { NUGET_SOURCES, NPM_SOURCES } from '@/types/packages'
import { searchPackages, getPackageVersions } from '@/api/packages'

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
const searchQuery = ref('')
const searchResults = ref<PackageInfo[]>([])
const searching = ref(false)
const selectedPackage = ref<PackageInfo | null>(null)
const versions = ref<string[]>([])
const loadingVersions = ref(false)
const selectedVersion = ref('')
const currentSource = ref<PackageSource>(
  props.managerType === 'nuget' ? NUGET_SOURCES[0] : NPM_SOURCES[0]
)
const showSourcePopover = ref(false)

// 计算属性
const sources = computed(() =>
  props.managerType === 'nuget' ? NUGET_SOURCES : NPM_SOURCES
)

const packages = computed(() => props.modelValue)

// 防抖搜索
let searchTimer: ReturnType<typeof setTimeout> | null = null

async function handleSearch(query: string) {
  searchQuery.value = query

  if (searchTimer) clearTimeout(searchTimer)

  if (!query.trim()) {
    searchResults.value = []
    return
  }

  searchTimer = setTimeout(async () => {
    searching.value = true
    try {
      const response = await searchPackages(
        props.managerType,
        query,
        currentSource.value.isDefault ? undefined : currentSource.value.url
      )
      searchResults.value = response.items
    } catch (e) {
      console.error('Search failed:', e)
      searchResults.value = []
    } finally {
      searching.value = false
    }
  }, 300)
}

async function handleSelectPackage(pkg: PackageInfo) {
  // 冲突检测
  const nameLower = pkg.name.toLowerCase()
  if (props.systemPackages.some(p => p.toLowerCase() === nameLower)) {
    ElMessage.warning(`包 "${pkg.name}" 已被系统模块使用`)
    return
  }
  if (packages.value.some(p => p.name.toLowerCase() === nameLower)) {
    ElMessage.warning(`包 "${pkg.name}" 已添加`)
    return
  }

  selectedPackage.value = pkg
  selectedVersion.value = pkg.version

  // 加载版本列表
  loadingVersions.value = true
  try {
    versions.value = await getPackageVersions(
      props.managerType,
      pkg.name,
      currentSource.value.isDefault ? undefined : currentSource.value.url
    )
    if (versions.value.length > 0 && !versions.value.includes(selectedVersion.value)) {
      selectedVersion.value = versions.value[0]
    }
  } catch (e) {
    console.error('Failed to load versions:', e)
    versions.value = [pkg.version]
  } finally {
    loadingVersions.value = false
  }
}

function handleAddPackage() {
  if (!selectedPackage.value || !selectedVersion.value) return

  const newPackage: PackageReference = {
    name: selectedPackage.value.name,
    version: selectedVersion.value,
    source: currentSource.value.isDefault ? undefined : currentSource.value.url
  }

  emit('update:modelValue', [...packages.value, newPackage])

  // 重置状态
  selectedPackage.value = null
  selectedVersion.value = ''
  searchQuery.value = ''
  searchResults.value = []
}

function handleRemovePackage(packageName: string) {
  emit('update:modelValue', packages.value.filter(p => p.name !== packageName))
}

function handleSourceChange(source: PackageSource) {
  currentSource.value = source
  showSourcePopover.value = false
  // 清空搜索结果
  searchResults.value = []
  if (searchQuery.value) {
    handleSearch(searchQuery.value)
  }
}
</script>

<template>
  <div class="package-selector">
    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-input
        v-model="searchQuery"
        :placeholder="`搜索 ${managerType === 'nuget' ? 'NuGet' : 'npm'} 包...`"
        clearable
        @input="handleSearch"
      >
        <template #append>
          <el-popover
            v-model:visible="showSourcePopover"
            placement="bottom-end"
            :width="200"
            trigger="click"
          >
            <template #reference>
              <el-button>
                {{ currentSource.name }}
                <el-icon class="el-icon--right"><arrow-down /></el-icon>
              </el-button>
            </template>
            <div class="source-list">
              <div
                v-for="source in sources"
                :key="source.url"
                class="source-item"
                :class="{ active: source.url === currentSource.url }"
                @click="handleSourceChange(source)"
              >
                {{ source.name }}
              </div>
            </div>
          </el-popover>
        </template>
      </el-input>
    </div>

    <!-- 搜索结果 -->
    <div v-if="searchResults.length > 0" class="search-results">
      <div
        v-for="pkg in searchResults"
        :key="pkg.name"
        class="result-item"
        @click="handleSelectPackage(pkg)"
      >
        <div class="pkg-name">{{ pkg.name }}</div>
        <div class="pkg-version">{{ pkg.version }}</div>
        <div class="pkg-desc">{{ pkg.description }}</div>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="searching" class="loading-hint">
      <el-icon class="is-loading"><loading /></el-icon>
      搜索中...
    </div>

    <!-- 版本选择 -->
    <div v-if="selectedPackage" class="version-selector">
      <span class="selected-pkg">{{ selectedPackage.name }}</span>
      <el-select
        v-model="selectedVersion"
        :loading="loadingVersions"
        size="small"
        style="width: 120px"
      >
        <el-option
          v-for="v in versions"
          :key="v"
          :label="v"
          :value="v"
        />
      </el-select>
      <el-button type="primary" size="small" @click="handleAddPackage">
        添加
      </el-button>
      <el-button size="small" @click="selectedPackage = null">
        取消
      </el-button>
    </div>

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
  </div>
</template>

<style scoped>
.package-selector {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.search-bar {
  display: flex;
}

.search-results {
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
}

.result-item {
  padding: 8px 12px;
  cursor: pointer;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.result-item:last-child {
  border-bottom: none;
}

.result-item:hover {
  background: var(--el-fill-color-light);
}

.pkg-name {
  font-weight: 500;
  color: var(--el-text-color-primary);
}

.pkg-version {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.pkg-desc {
  font-size: 12px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.loading-hint {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.version-selector {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  background: var(--el-fill-color-lighter);
  border-radius: 4px;
}

.selected-pkg {
  font-weight: 500;
}

.selected-packages {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.source-list {
  display: flex;
  flex-direction: column;
}

.source-item {
  padding: 8px 12px;
  cursor: pointer;
  border-radius: 4px;
}

.source-item:hover {
  background: var(--el-fill-color-light);
}

.source-item.active {
  color: var(--el-color-primary);
  background: var(--el-color-primary-light-9);
}
</style>
