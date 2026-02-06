<template>
  <div class="home-page">
    <header class="page-header">
      <h1>Scaffold Generator</h1>
      <p>快速生成 .NET + Vue 全栈项目脚手架</p>
    </header>

    <main class="page-content">
      <div class="left-panel">
        <ConfigForm />
      </div>
      <div class="right-panel">
        <FileTreeView />
      </div>
    </main>

    <footer class="page-footer">
      <p>Powered by .NET 9 + Vue 3 + Element Plus</p>
    </footer>
  </div>
</template>

<script setup lang="ts">
import ConfigForm from '@/components/ConfigForm.vue'
import FileTreeView from '@/components/FileTreeView.vue'
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.page-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 32px;
  text-align: center;
}

.page-header h1 {
  font-size: 32px;
  margin-bottom: 8px;
}

.page-header p {
  font-size: 16px;
  opacity: 0.9;
}

.page-content {
  flex: 1;
  display: flex;
  gap: 24px;
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
}

.left-panel {
  flex: 1;
  min-width: 400px;
}

.right-panel {
  width: 400px;
  flex-shrink: 0;
}

.page-footer {
  background: #303133;
  color: #909399;
  text-align: center;
  padding: 16px;
  font-size: 14px;
}

@media (max-width: 900px) {
  .page-content {
    flex-direction: column;
  }

  .left-panel,
  .right-panel {
    width: 100%;
    min-width: unset;
  }
}
</style>
