<template>
  <el-drawer
    v-model="showDrawer"
    direction="rtl"
    :size="drawerWidth"
    :show-close="true"
    :with-header="true"
    title="Preview"
    class="preview-drawer"
    @close="store.closePreview()"
  >
    <!-- Drawer 左边缘拖拽条 -->
    <div
      class="drawer-resize-handle"
      @mousedown="startDrawerResize"
    />

    <div class="preview-split">
      <div class="explorer-panel" :style="{ width: explorerWidth + 'px' }">
        <div class="panel-header">Explorer</div>
        <div class="panel-content">
          <FileTreeView theme="dark" />
        </div>
      </div>

      <!-- Explorer 右边缘拖拽条 -->
      <div
        class="panel-resize-handle"
        @mousedown="startExplorerResize"
      />

      <div class="code-panel">
        <div class="panel-header">Code</div>
        <div class="panel-content">
          <CodePreview />
        </div>
      </div>
    </div>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useConfigStore } from '@/stores/config'
import FileTreeView from '@/components/FileTreeView.vue'
import CodePreview from '@/components/CodePreview.vue'

const store = useConfigStore()
const { showPreviewDrawer } = storeToRefs(store)

// Drawer 宽度 (百分比字符串)
const drawerWidthPercent = ref(75)
const drawerWidth = computed(() => `${drawerWidthPercent.value}%`)

// Explorer 面板宽度 (像素)
const explorerWidth = ref(320)

// 拖拽状态
const isResizingDrawer = ref(false)
const isResizingExplorer = ref(false)

const showDrawer = computed({
  get: () => showPreviewDrawer.value,
  set: (val: boolean) => {
    if (!val) store.closePreview()
  }
})

// Drawer 宽度拖拽
function startDrawerResize(e: MouseEvent) {
  e.preventDefault()
  isResizingDrawer.value = true
  document.addEventListener('mousemove', onDrawerResize)
  document.addEventListener('mouseup', stopDrawerResize)
  document.body.style.cursor = 'ew-resize'
  document.body.style.userSelect = 'none'
}

function onDrawerResize(e: MouseEvent) {
  if (!isResizingDrawer.value) return
  const windowWidth = window.innerWidth
  const newWidth = ((windowWidth - e.clientX) / windowWidth) * 100
  // 限制范围: 30% - 90%
  drawerWidthPercent.value = Math.max(30, Math.min(90, newWidth))
}

function stopDrawerResize() {
  isResizingDrawer.value = false
  document.removeEventListener('mousemove', onDrawerResize)
  document.removeEventListener('mouseup', stopDrawerResize)
  document.body.style.cursor = ''
  document.body.style.userSelect = ''
}

// Explorer 面板宽度拖拽
function startExplorerResize(e: MouseEvent) {
  e.preventDefault()
  isResizingExplorer.value = true
  document.addEventListener('mousemove', onExplorerResize)
  document.addEventListener('mouseup', stopExplorerResize)
  document.body.style.cursor = 'ew-resize'
  document.body.style.userSelect = 'none'
}

function onExplorerResize(e: MouseEvent) {
  if (!isResizingExplorer.value) return
  const drawerEl = document.querySelector('.el-drawer__body')
  if (!drawerEl) return
  const drawerRect = drawerEl.getBoundingClientRect()
  const newWidth = e.clientX - drawerRect.left
  // 限制范围: 150px - 500px
  explorerWidth.value = Math.max(150, Math.min(500, newWidth))
}

function stopExplorerResize() {
  isResizingExplorer.value = false
  document.removeEventListener('mousemove', onExplorerResize)
  document.removeEventListener('mouseup', stopExplorerResize)
  document.body.style.cursor = ''
  document.body.style.userSelect = ''
}

// 清理事件监听
onUnmounted(() => {
  document.removeEventListener('mousemove', onDrawerResize)
  document.removeEventListener('mouseup', stopDrawerResize)
  document.removeEventListener('mousemove', onExplorerResize)
  document.removeEventListener('mouseup', stopExplorerResize)
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
  height: 100%;
  position: relative;
}

/* Drawer 左边缘拖拽条 */
.drawer-resize-handle {
  position: absolute;
  left: 0;
  top: 0;
  width: 4px;
  height: 100%;
  cursor: ew-resize;
  background-color: transparent;
  z-index: 10;
  transition: background-color 0.2s;
}

.drawer-resize-handle:hover,
.drawer-resize-handle:active {
  background-color: #4f46e5;
}

.preview-split {
  display: flex;
  height: 100%;
  margin-left: 4px;
}

.explorer-panel {
  min-width: 150px;
  max-width: 500px;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}

/* Explorer 右边缘拖拽条 */
.panel-resize-handle {
  width: 4px;
  cursor: ew-resize;
  background-color: #333;
  flex-shrink: 0;
  transition: background-color 0.2s;
}

.panel-resize-handle:hover,
.panel-resize-handle:active {
  background-color: #4f46e5;
}

.code-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 200px;
}

.panel-header {
  background-color: #252526;
  color: #808080;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  padding: 8px 12px;
  border-bottom: 1px solid #333;
  flex-shrink: 0;
}

.panel-content {
  flex: 1;
  overflow: hidden;
}

.explorer-panel .panel-content {
  overflow-y: auto;
  background-color: #252526;
}

.code-panel .panel-content {
  background-color: #1e1e1e;
}
</style>
