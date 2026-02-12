<template>
  <div class="file-tree-view" :class="theme">
    <!-- Loading skeleton -->
    <div v-if="treeLoading" class="skeleton-container">
      <div v-for="i in 6" :key="i" class="skeleton-item">
        <el-skeleton :rows="0" animated :class="{ 'dark-skeleton': theme === 'dark' }">
          <template #template>
            <el-skeleton-item variant="text" :style="{ width: `${60 + i * 15}%` }" />
          </template>
        </el-skeleton>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else-if="!fileTree || fileTree.length === 0" class="empty-state">
      <el-icon class="empty-icon"><FolderOpened /></el-icon>
      <p class="empty-text">配置项目预览</p>
    </div>

    <!-- File tree -->
    <el-tree
      v-else
      :data="fileTree"
      :props="treeProps"
      default-expand-all
      :expand-on-click-node="false"
      node-key="path"
      :current-node-key="store.selectedFile?.path"
      highlight-current
      class="file-tree"
      @node-click="handleNodeClick"
    >
      <template #default="{ node, data }">
        <span class="tree-node">
          <el-icon v-if="data.isDirectory" class="folder-icon">
            <Folder />
          </el-icon>
          <el-icon v-else class="file-icon">
            <Document />
          </el-icon>
          <span class="node-label">{{ node.label }}</span>
        </span>
      </template>
    </el-tree>
  </div>
</template>

<script setup lang="ts">
import { useConfigStore } from '@/stores/config'
import { storeToRefs } from 'pinia'
import { Folder, Document, FolderOpened } from '@element-plus/icons-vue'
import type { FileTreeNode } from '@/types'

withDefaults(defineProps<{
  theme?: 'light' | 'dark'
}>(), {
  theme: 'light'
})

const store = useConfigStore()
const { fileTree, treeLoading, selectedFile } = storeToRefs(store)

const treeProps = {
  label: 'name',
  children: 'children'
}

function handleNodeClick(data: FileTreeNode) {
  if (!data.isDirectory) {
    store.selectFile(data)
  }
}
</script>

<style scoped>
.file-tree-view {
  height: 100%;
  padding: 8px 0;
}

/* Skeleton */
.skeleton-container {
  padding: 8px 16px;
}
.skeleton-item {
  padding: 4px 0;
}
.dark-skeleton {
  --el-skeleton-color: #333;
  --el-skeleton-to-color: #444;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  color: #909399;
}
.file-tree-view.dark .empty-state {
  color: #6e7681;
}
.empty-icon {
  font-size: 32px;
  margin-bottom: 8px;
  opacity: 0.5;
}
.empty-text {
  font-size: 13px;
  margin: 0;
}

/* Tree Styling */
.file-tree {
  background: transparent;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 13px;
}

.tree-node {
  display: flex;
  align-items: center;
  gap: 6px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.folder-icon {
  color: #e6a23c;
}

.file-icon {
  color: #909399;
}

/* Dark Theme Overrides for Element Plus Tree */
.file-tree-view.dark .file-tree {
  color: #cccccc;
  background: transparent;
  --el-tree-node-content-height: 24px;
  --el-tree-node-hover-bg-color: #2a2d2e;
}

.file-tree-view.dark .folder-icon {
  color: #dcb67a;
}

.file-tree-view.dark .file-icon {
  color: #8b949e;
}

/* Deep selector for tree node content */
:deep(.el-tree-node__content) {
  border-radius: 0;
}

.file-tree-view.dark :deep(.el-tree-node:focus > .el-tree-node__content),
.file-tree-view.dark :deep(.el-tree-node.is-current > .el-tree-node__content) {
  background-color: #37373d !important; /* Selected state */
  color: #ffffff;
}
</style>
