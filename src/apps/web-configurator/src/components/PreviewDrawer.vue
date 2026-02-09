<template>
  <el-drawer
    v-model="showDrawer"
    direction="rtl"
    size="50%"
    :show-close="true"
    :with-header="true"
    title="Preview"
    class="preview-drawer"
    @close="store.closePreview()"
  >
    <el-tabs v-model="activeTab" class="preview-tabs">
      <el-tab-pane label="Explorer" name="explorer">
        <div class="explorer-pane">
          <FileTreeView theme="dark" />
        </div>
      </el-tab-pane>
      <el-tab-pane label="Code" name="code">
        <div class="code-pane">
          <CodePreview />
        </div>
      </el-tab-pane>
    </el-tabs>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'
import FileTreeView from '@/components/FileTreeView.vue'
import CodePreview from '@/components/CodePreview.vue'

const store = useConfigStore()
const { showPreviewDrawer, selectedFile } = storeToRefs(store)

const activeTab = ref<'explorer' | 'code'>('explorer')

const showDrawer = computed({
  get: () => showPreviewDrawer.value,
  set: (val: boolean) => {
    if (!val) store.closePreview()
  }
})

// 当选中文件时自动切换到 Code Tab
watch(selectedFile, (newFile) => {
  if (newFile && !newFile.isDirectory) {
    activeTab.value = 'code'
  }
})
</script>

<style scoped>
.preview-drawer :deep(.el-drawer__header) {
  background-color: #252526;
  color: #cccccc;
  margin-bottom: 0;
  padding: 12px 16px;
  border-bottom: 1px solid #333;
}

.preview-drawer :deep(.el-drawer__body) {
  background-color: #1e1e1e;
  padding: 0;
}

.preview-tabs {
  height: 100%;
}

.preview-tabs :deep(.el-tabs__header) {
  background-color: #252526;
  margin: 0;
  padding: 0 16px;
}

.preview-tabs :deep(.el-tabs__item) {
  color: #808080;
}

.preview-tabs :deep(.el-tabs__item.is-active) {
  color: #ffffff;
}

.preview-tabs :deep(.el-tabs__nav-wrap::after) {
  background-color: #333;
}

.preview-tabs :deep(.el-tabs__active-bar) {
  background-color: #4f46e5;
}

.preview-tabs :deep(.el-tabs__content) {
  height: calc(100% - 40px);
  padding: 0;
}

.preview-tabs :deep(.el-tab-pane) {
  height: 100%;
}

.explorer-pane {
  height: 100%;
  overflow-y: auto;
  background-color: #252526;
}

.code-pane {
  height: 100%;
  overflow: hidden;
  background-color: #1e1e1e;
}
</style>
