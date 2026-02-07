<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useConfigStore } from '@/stores/config'
import { useShiki } from '@/composables/useShiki'
import { ElMessage } from 'element-plus'
import { DocumentCopy } from '@element-plus/icons-vue'

const store = useConfigStore()
const { highlight, loading: shikiLoading } = useShiki()

const highlightedCode = ref('')
const copied = ref(false)

const isLoading = computed(() => store.previewLoading || shikiLoading.value)

watch(
  () => store.previewContent,
  async (content) => {
    if (content?.content) {
      highlightedCode.value = await highlight(content.content, content.language)
    } else {
      highlightedCode.value = ''
    }
  },
  { immediate: true }
)

async function copyCode() {
  if (!store.previewContent?.content) return
  try {
    await navigator.clipboard.writeText(store.previewContent.content)
    copied.value = true
    ElMessage.success('已复制')
    setTimeout(() => {
      copied.value = false
    }, 2000)
  } catch {
    ElMessage.error('复制失败')
  }
}
</script>

<template>
  <div class="code-preview">
    <!-- Header / Tab Bar -->
    <div class="code-preview-header">
      <div class="file-tab" v-if="store.selectedFile">
        <span class="file-icon-dot"></span>
        <span class="file-name">{{ store.selectedFile.name }}</span>
      </div>
      <div v-else class="file-tab placeholder">
        <span>Preview</span>
      </div>

      <div class="header-actions">
        <button
          class="copy-btn"
          :class="{ copied }"
          @click="copyCode"
          :disabled="!store.previewContent?.content"
          :title="copied ? '已复制' : '复制代码'"
        >
          <DocumentCopy class="copy-icon" />
          <span class="copy-text">{{ copied ? 'Copied' : 'Copy' }}</span>
        </button>
      </div>
    </div>

    <!-- Code Body -->
    <div class="code-preview-body">
      <div v-if="isLoading" class="loading-state">
        <div class="loading-spinner"></div>
        <span>加载中...</span>
      </div>
      <div v-else-if="!store.selectedFile" class="empty-state">
        <div class="empty-icon">{ }</div>
        <p>Select a file to view source</p>
      </div>
      <div v-else-if="!store.previewContent" class="empty-state error">
        <p>无法加载预览</p>
      </div>
      <div v-else class="code-content" v-html="highlightedCode"></div>
    </div>
  </div>
</template>

<style scoped>
.code-preview {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #1e1e1e;
  overflow: hidden;
}

.code-preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 36px;
  background: #252526; /* Tab bar background */
  border-bottom: 1px solid #333;
}

.file-tab {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 16px;
  height: 100%;
  background: #1e1e1e; /* Active tab color */
  border-right: 1px solid #333;
  color: #fff;
  font-size: 13px;
  min-width: 120px;
}

.file-tab.placeholder {
  background: transparent;
  color: #666;
  font-style: italic;
}

.file-icon-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #e06c75; /* Red dot for 'unsaved' look or just accent */
}

.file-name {
  font-family: 'Segoe UI', sans-serif;
}

.header-actions {
  padding-right: 12px;
}

.copy-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 12px;
  height: 24px;
  border: none;
  border-radius: 20px; /* Rounded Pill */
  background: rgba(255, 255, 255, 0.1);
  color: #cccccc;
  font-size: 11px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.copy-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.2);
  color: #ffffff;
}

.copy-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.copy-btn.copied {
  background: #4caf50;
  color: white;
}

.copy-icon {
  width: 12px;
  height: 12px;
}

.code-preview-body {
  flex: 1;
  overflow: auto;
  background: #1e1e1e;
  position: relative;
}

.loading-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #5c6370;
  gap: 16px;
}

.loading-spinner {
  width: 24px;
  height: 24px;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-top-color: #409eff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.empty-icon {
  font-size: 64px;
  font-family: monospace;
  opacity: 0.1;
  color: #fff;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}

.code-content {
  padding: 20px;
  font-family: 'Fira Code', 'SF Mono', 'Consolas', monospace;
  font-size: 14px;
  line-height: 1.6;
}

.code-content :deep(pre) {
  margin: 0;
  background: transparent !important;
  overflow-x: auto;
}

.code-content :deep(code) {
  font-family: inherit;
  background: transparent !important;
}

/* Scrollbar styling */
.code-preview-body::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

.code-preview-body::-webkit-scrollbar-track {
  background: #1e1e1e;
}

.code-preview-body::-webkit-scrollbar-thumb {
  background: #424242;
  border-radius: 5px;
  border: 2px solid #1e1e1e;
}

.code-preview-body::-webkit-scrollbar-thumb:hover {
  background: #4f4f4f;
}
</style>
