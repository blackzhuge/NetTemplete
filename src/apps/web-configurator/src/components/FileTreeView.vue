<template>
  <el-card class="file-tree-view">
    <template #header>
      <span class="card-title">生成预览</span>
    </template>

    <el-tree
      :data="fileTree"
      :props="treeProps"
      default-expand-all
      :expand-on-click-node="false"
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
import { useConfigStore } from '@/stores/config'
import { storeToRefs } from 'pinia'
import { Folder, Document } from '@element-plus/icons-vue'

const store = useConfigStore()
const { fileTree } = storeToRefs(store)

const treeProps = {
  label: 'name',
  children: 'children'
}
</script>

<style scoped>
.file-tree-view {
  height: 100%;
}

.card-title {
  font-weight: 600;
}

.tree-node {
  display: flex;
  align-items: center;
  gap: 6px;
}

.folder-icon {
  color: #e6a23c;
}

.file-icon {
  color: #909399;
}
</style>
