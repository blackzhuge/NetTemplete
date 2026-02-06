<template>
  <div class="home-page">
    <header class="page-header">
      <div class="header-content">
        <div class="logo">
          <el-icon class="logo-icon"><Box /></el-icon>
          <h1>Scaffold Generator</h1>
        </div>
        <p class="subtitle">快速生成 .NET + Vue 全栈项目脚手架</p>
      </div>
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
      <div class="footer-content">
        <div class="tech-stack">
          <el-tag type="info" effect="plain">.NET 9</el-tag>
          <el-tag type="success" effect="plain">Vue 3</el-tag>
          <el-tag type="warning" effect="plain">Element Plus</el-tag>
        </div>
        <p class="copyright">Scaffold Generator - 让项目初始化更简单</p>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { Box } from '@element-plus/icons-vue'
import ConfigForm from '@/components/ConfigForm.vue'
import FileTreeView from '@/components/FileTreeView.vue'
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f5f7fa;
}

.page-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 32px;
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  text-align: center;
}

.logo {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-bottom: 8px;
}

.logo-icon {
  font-size: 36px;
}

.logo h1 {
  font-size: 32px;
  margin: 0;
  font-weight: 700;
}

.subtitle {
  font-size: 16px;
  opacity: 0.9;
  margin: 0;
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
  padding: 20px;
}

.footer-content {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.tech-stack {
  display: flex;
  gap: 8px;
}

.copyright {
  font-size: 13px;
  margin: 0;
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
