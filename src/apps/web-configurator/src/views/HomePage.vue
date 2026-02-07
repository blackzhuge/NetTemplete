<template>
  <div class="home-page">
    <!-- Left Sidebar: Configuration -->
    <aside class="config-sidebar">
      <header class="sidebar-header">
        <div class="logo">
          <el-icon class="logo-icon"><Box /></el-icon>
          <h1>Scaffold Generator</h1>
        </div>
        <p class="subtitle">.NET + Vue 全栈项目生成器</p>
      </header>

      <div class="sidebar-content">
        <PresetSelector />
        <el-divider class="sidebar-divider" />
        <ConfigForm />
      </div>

      <footer class="sidebar-footer">
        <div class="tech-stack">
          <el-tag size="small" type="info" effect="plain">.NET 9</el-tag>
          <el-tag size="small" type="success" effect="plain">Vue 3</el-tag>
          <el-tag size="small" type="warning" effect="plain">Element+</el-tag>
        </div>
      </footer>
    </aside>

    <!-- Right Area: IDE Preview -->
    <main class="ide-preview">
      <!-- File Explorer Pane -->
      <div class="ide-sidebar">
        <div class="ide-pane-header">
          <span class="pane-title">EXPLORER</span>
        </div>
        <div class="file-tree-container">
          <FileTreeView theme="dark" />
        </div>
      </div>

      <!-- Editor Pane -->
      <div class="ide-editor">
        <CodePreview />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { Box } from '@element-plus/icons-vue'
import ConfigForm from '@/components/ConfigForm.vue'
import FileTreeView from '@/components/FileTreeView.vue'
import PresetSelector from '@/components/PresetSelector.vue'
import CodePreview from '@/components/CodePreview.vue'
</script>

<style scoped>
.home-page {
  height: 100vh;
  display: flex;
  overflow: hidden;
  background-color: #f8fafc;
}

/* Left Sidebar */
.config-sidebar {
  width: 420px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  background: white;
  border-right: 1px solid #e2e8f0;
  z-index: 10;
  box-shadow: 4px 0 24px rgba(0,0,0,0.02);
}

.sidebar-header {
  padding: 24px 32px;
  background: white;
  border-bottom: 1px solid #f1f5f9;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 4px;
  color: #0f172a;
}

.logo-icon {
  font-size: 26px;
  color: #4f46e5;
}

.logo h1 {
  font-size: 20px;
  margin: 0;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.subtitle {
  font-size: 13px;
  color: #64748b;
  margin: 0;
  padding-left: 36px;
}

.sidebar-content {
  flex: 1;
  overflow-y: auto;
  padding: 24px 32px;
}

.sidebar-divider {
  margin: 24px 0;
}

.sidebar-footer {
  padding: 16px 32px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: center;
}

.tech-stack {
  display: flex;
  gap: 8px;
}

/* Right IDE Area */
.ide-preview {
  flex: 1;
  display: flex;
  background-color: #1e1e1e; /* Dark IDE background */
  overflow: hidden;
}

.ide-sidebar {
  width: 260px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  background-color: #252526; /* VS Code Sidebar Color */
  border-right: 1px solid #333;
}

.ide-pane-header {
  height: 35px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  font-size: 11px;
  font-weight: 600;
  color: #bbbbbb;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.file-tree-container {
  flex: 1;
  overflow-y: auto;
}

.ide-editor {
  flex: 1;
  min-width: 0; /* Prevent overflow */
  background-color: #1e1e1e;
  display: flex;
  flex-direction: column;
}

/* Scrollbar Styling for Sidebar */
.sidebar-content::-webkit-scrollbar {
  width: 6px;
}
.sidebar-content::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}
.sidebar-content::-webkit-scrollbar-track {
  background: transparent;
}
</style>
