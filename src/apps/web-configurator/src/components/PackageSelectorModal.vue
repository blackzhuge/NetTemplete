<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import type { PackageInfo, PackageReference, PackageManagerType, PackageSource } from '@/types/packages'
import { NUGET_SOURCES, NPM_SOURCES } from '@/types/packages'
import { searchPackages, getPackageVersions } from '@/api/packages'

interface Props {
  visible: boolean
  managerType: PackageManagerType
  existingPackages: PackageReference[]
  systemPackages?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  systemPackages: () => []
})

const emit = defineEmits<{
  'update:visible': [visible: boolean]
  'confirm': [packages: PackageReference[]]
}>()

// 状态
const searchQuery = ref('')
const searchResults = ref<PackageInfo[]>([])
const searching = ref(false)
const selectedPackages = ref<Map<string, PackageReference>>(new Map())
const versionsMap = ref<Map<string, string[]>>(new Map())
const loadingVersions = ref<Set<string>>(new Set())
const currentSource = ref<PackageSource>(
  props.managerType === 'nuget' ? NUGET_SOURCES[0] : NPM_SOURCES[0]
)
const sortBy = ref<'relevance' | 'downloads'>('downloads')

// 计算属性
const sources = computed(() =>
  props.managerType === 'nuget' ? NUGET_SOURCES : NPM_SOURCES
)

const dialogVisible = computed({
  get: () => props.visible,
  set: (val: boolean) => emit('update:visible', val)
})

const sortedResults = computed(() => {
  if (sortBy.value === 'downloads') {
    return [...searchResults.value].sort((a, b) => (b.downloadCount ?? 0) - (a.downloadCount ?? 0))
  }
  return searchResults.value
})

const selectedList = computed(() => Array.from(selectedPackages.value.values()))

// 防抖搜索
let searchTimer: ReturnType<typeof setTimeout> | null = null

async function handleSearch() {
  const query = searchQuery.value

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

function isPackageDisabled(pkg: PackageInfo): boolean {
  const nameLower = pkg.name.toLowerCase()
  // 检查系统包
  if (props.systemPackages.some(p => p.toLowerCase() === nameLower)) {
    return true
  }
  // 检查已存在的包
  if (props.existingPackages.some(p => p.name.toLowerCase() === nameLower)) {
    return true
  }
  return false
}

function isPackageSelected(pkg: PackageInfo): boolean {
  return selectedPackages.value.has(pkg.name.toLowerCase())
}

async function handleTogglePackage(pkg: PackageInfo) {
  const nameLower = pkg.name.toLowerCase()

  if (isPackageDisabled(pkg)) {
    ElMessage.warning(`包 "${pkg.name}" 已被系统模块使用或已添加`)
    return
  }

  if (selectedPackages.value.has(nameLower)) {
    selectedPackages.value.delete(nameLower)
    return
  }

  // 添加到临时选中列表
  selectedPackages.value.set(nameLower, {
    name: pkg.name,
    version: pkg.version,
    source: currentSource.value.isDefault ? undefined : currentSource.value.url
  })

  // 加载版本列表
  if (!versionsMap.value.has(nameLower)) {
    loadingVersions.value.add(nameLower)
    try {
      const versions = await getPackageVersions(
        props.managerType,
        pkg.name,
        currentSource.value.isDefault ? undefined : currentSource.value.url
      )
      versionsMap.value.set(nameLower, versions)
    } catch (e) {
      console.error('Failed to load versions:', e)
      versionsMap.value.set(nameLower, [pkg.version])
    } finally {
      loadingVersions.value.delete(nameLower)
    }
  }
}

function handleVersionChange(packageName: string, version: string) {
  const nameLower = packageName.toLowerCase()
  const pkg = selectedPackages.value.get(nameLower)
  if (pkg) {
    selectedPackages.value.set(nameLower, { ...pkg, version })
  }
}

function handleRemoveSelected(packageName: string) {
  selectedPackages.value.delete(packageName.toLowerCase())
}

function handleConfirm() {
  emit('confirm', selectedList.value)
  handleClose()
}

function handleClose() {
  dialogVisible.value = false
  // 重置状态
  searchQuery.value = ''
  searchResults.value = []
  selectedPackages.value.clear()
}

function formatDownloads(count?: number): string {
  if (!count) return ''
  if (count >= 1000000) return `${(count / 1000000).toFixed(1)}M`
  if (count >= 1000) return `${(count / 1000).toFixed(1)}K`
  return String(count)
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-CN')
}

// 重置状态当 visible 变化
watch(() => props.visible, (visible) => {
  if (visible) {
    currentSource.value = props.managerType === 'nuget' ? NUGET_SOURCES[0] : NPM_SOURCES[0]
  }
})
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    :title="`添加 ${managerType === 'nuget' ? 'NuGet' : 'npm'} 依赖`"
    width="700px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <!-- 搜索栏 -->
    <div class="search-header">
      <el-input
        v-model="searchQuery"
        :placeholder="`搜索 ${managerType === 'nuget' ? 'NuGet' : 'npm'} 包...`"
        clearable
        :prefix-icon="Search"
        @input="handleSearch"
      />
      <el-select v-model="currentSource" value-key="url" style="width: 140px">
        <el-option
          v-for="source in sources"
          :key="source.url"
          :label="source.name"
          :value="source"
        />
      </el-select>
      <el-select v-model="sortBy" style="width: 120px">
        <el-option label="按下载量" value="downloads" />
        <el-option label="按相关性" value="relevance" />
      </el-select>
    </div>

    <!-- 搜索结果 -->
    <div class="search-results">
      <div v-if="searching" class="loading-state">
        <el-icon class="is-loading"><loading /></el-icon>
        搜索中...
      </div>
      <div v-else-if="sortedResults.length === 0 && searchQuery" class="empty-state">
        未找到匹配的包
      </div>
      <div
        v-for="pkg in sortedResults"
        :key="pkg.name"
        class="result-item"
        :class="{
          selected: isPackageSelected(pkg),
          disabled: isPackageDisabled(pkg)
        }"
        @click="handleTogglePackage(pkg)"
      >
        <div class="pkg-info">
          <div class="pkg-header">
            <span class="pkg-name">{{ pkg.name }}</span>
            <span class="pkg-version">{{ pkg.version }}</span>
          </div>
          <div class="pkg-desc">{{ pkg.description }}</div>
          <div class="pkg-meta">
            <span v-if="pkg.downloadCount" class="meta-item">
              <el-icon><download /></el-icon>
              {{ formatDownloads(pkg.downloadCount) }}
            </span>
            <span v-if="pkg.lastUpdated" class="meta-item">
              {{ formatDate(pkg.lastUpdated) }}
            </span>
          </div>
        </div>
        <el-icon v-if="isPackageSelected(pkg)" class="check-icon"><check /></el-icon>
      </div>
    </div>

    <!-- 已选包列表 -->
    <div v-if="selectedList.length > 0" class="selected-section">
      <div class="section-title">已选择 ({{ selectedList.length }})</div>
      <div class="selected-list">
        <div v-for="pkg in selectedList" :key="pkg.name" class="selected-item">
          <span class="pkg-name">{{ pkg.name }}</span>
          <el-select
            :model-value="pkg.version"
            size="small"
            :loading="loadingVersions.has(pkg.name.toLowerCase())"
            style="width: 100px"
            @update:model-value="handleVersionChange(pkg.name, $event)"
          >
            <el-option
              v-for="v in versionsMap.get(pkg.name.toLowerCase()) || [pkg.version]"
              :key="v"
              :label="v"
              :value="v"
            />
          </el-select>
          <el-button
            type="danger"
            size="small"
            text
            @click.stop="handleRemoveSelected(pkg.name)"
          >
            移除
          </el-button>
        </div>
      </div>
    </div>

    <template #footer>
      <el-button @click="handleClose">取消</el-button>
      <el-button
        type="primary"
        :disabled="selectedList.length === 0"
        @click="handleConfirm"
      >
        添加 {{ selectedList.length > 0 ? `(${selectedList.length})` : '' }}
      </el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.search-header {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
}

.search-header .el-input {
  flex: 1;
}

.search-results {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  margin-bottom: 16px;
}

.loading-state,
.empty-state {
  padding: 24px;
  text-align: center;
  color: var(--el-text-color-secondary);
}

.result-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  cursor: pointer;
  border-bottom: 1px solid var(--el-border-color-lighter);
  transition: background-color 0.2s;
}

.result-item:last-child {
  border-bottom: none;
}

.result-item:hover {
  background: var(--el-fill-color-light);
}

.result-item.selected {
  background: var(--el-color-primary-light-9);
}

.result-item.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pkg-info {
  flex: 1;
  min-width: 0;
}

.pkg-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
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
  font-size: 13px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 4px;
}

.pkg-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.check-icon {
  color: var(--el-color-primary);
  font-size: 18px;
}

.selected-section {
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  padding: 12px;
}

.section-title {
  font-weight: 500;
  margin-bottom: 8px;
  color: var(--el-text-color-primary);
}

.selected-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.selected-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  background: var(--el-fill-color-lighter);
  border-radius: 4px;
}

.selected-item .pkg-name {
  flex: 1;
}
</style>
