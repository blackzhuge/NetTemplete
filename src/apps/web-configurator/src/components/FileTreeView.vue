<template>
  <el-card class="file-tree-view">
    <template #header>
      <div class="card-header">
        <el-icon class="header-icon"><Files /></el-icon>
        <span class="card-title">生成预览</span>
        <el-tag v-if="fileCount > 0" size="small" type="info">{{ fileCount }} 文件</el-tag>
      </div>
    </template>

    <!-- Loading skeleton -->
    <div v-if="loading" class="skeleton-container">
      <div v-for="i in 6" :key="i" class="skeleton-item">
        <el-skeleton :rows="0" animated>
          <template #template>
            <el-skeleton-item variant="text" :style="{ width: `${60 + i * 15}%` }" />
          </template>
        </el-skeleton>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else-if="!fileTree || fileTree.length === 0" class="empty-state">
      <el-icon class="empty-icon"><FolderOpened /></el-icon>
      <p class="empty-text">配置项目后预览文件结构</p>
    </div>

    <!-- File tree -->
    <el-tree
      v-else
      :data="fileTree"
      :props="treeProps"
      default-expand-all
      :expand-on-click-node="false"
      class="file-tree"
    >
      <template #default="{ node, data }">
        <span class="tree-node">
          <el-icon v-if="data.isDirectory" class="folder-icon">
            <Folder />
          </el-icon>
          <el-icon v-else class="file-icon">
            <Document />
          </el-icon>
          <span>{{ node.label }}</span>
        </span>
      </template>
    </el-tree>
  </el-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useConfigStore } from '@/stores/config'
import { storeToRefs } from 'pinia'
import { Folder, Document, Files, FolderOpened } from '@element-plus/icons-vue'
import type { FileTreeNode } from '@/types'

const store = useConfigStore()
const { fileTree, loading } = storeToRefs(store)

const treeProps = {
  label: 'name',
  children: 'children'
}

function countFiles(nodes: FileTreeNode[]): number {
  let count = 0
  for (const node of nodes) {
    if (!node.isDirectory) count++
    if (node.children) count += countFiles(node.children)
  }
  return count
}

const fileCount = computed(() => {
  if (!fileTree.value) return 0
  return countFiles(fileTree.value)
})
</script>

<style scoped>
.file-tree-view {
  height: 100%;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-icon {
  color: #409eff;
  font-size: 18px;
}

.card-title {
  font-weight: 600;
  flex: 1;
}

.skeleton-container {
  padding: 8px 0;
}

.skeleton-item {
  padding: 8px 0;
  padding-left: 24px;
}

.skeleton-item:nth-child(2),
.skeleton-item:nth-child(4) {
  padding-left: 48px;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  color: #909399;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty-text {
  font-size: 14px;
  margin: 0;
}

.file-tree {
  --el-tree-node-hover-bg-color: #f5f7fa;
}

.tree-node {
  display: flex;
  align-items: center;
  gap: 6px;
  transition: color 0.2s;
}

.tree-node:hover {
  color: #409eff;
}

.folder-icon {
  color: #e6a23c;
}

.file-icon {
  color: #909399;
}
</style>
