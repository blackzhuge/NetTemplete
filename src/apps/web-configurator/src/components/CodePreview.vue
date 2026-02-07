<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useConfigStore } from '@/stores/config'
import { useShiki } from '@/composables/useShiki'
import { ElMessage } from 'element-plus'

const store = useConfigStore()
const { highlight, loading: shikiLoading } = useShiki()

const highlightedCode = ref('')

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
    ElMessage.success('代码已复制到剪贴板')
  } catch {
    ElMessage.error('复制失败')
  }
}
</script>

<template>
  <div class="code-preview">
    <div class="code-preview-header" v-if="store.selectedFile">
      <span class="file-name">{{ store.selectedFile.name }}</span>
      <el-button
        size="small"
        :icon="'DocumentCopy'"
        @click="copyCode"
        :disabled="!store.previewContent?.content"
      >
        复制
      </el-button>
    </div>

    <div class="code-preview-body">
      <div v-if="isLoading" class="loading-state" v-loading="true">
        加载中...
      </div>
      <div v-else-if="!store.selectedFile" class="empty-state">
        点击左侧文件查看代码预览
      </div>
      <div v-else-if="!store.previewContent" class="empty-state">
        无法加载预览
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
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  overflow: hidden;
}

.code-preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f5f7fa;
  border-bottom: 1px solid #e4e7ed;
}

.file-name {
  font-weight: 500;
  font-size: 14px;
  color: #303133;
}

.code-preview-body {
  flex: 1;
  overflow: auto;
  background: #fafafa;
}

.loading-state,
.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 200px;
  color: #909399;
}

.code-content {
  padding: 12px;
  font-family: 'Fira Code', 'Consolas', monospace;
  font-size: 13px;
  line-height: 1.6;
}

.code-content :deep(pre) {
  margin: 0;
  background: transparent !important;
}

.code-content :deep(code) {
  font-family: inherit;
}

.code-content :deep(.line) {
  display: block;
}
</style>
