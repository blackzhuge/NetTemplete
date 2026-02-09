<template>
  <div class="home-page">
    <!-- Configuration Panel -->
    <main class="config-panel">
      <header class="panel-header">
        <div class="header-top">
          <div class="logo">
            <el-icon class="logo-icon"><Box /></el-icon>
            <h1>Scaffold Generator</h1>
          </div>
          <el-button type="primary" @click="store.openPreview()">
            <el-icon><View /></el-icon>
            预览
          </el-button>
        </div>
        <p class="subtitle">.NET + Vue 全栈项目生成器</p>
      </header>

      <div class="panel-content">
        <PresetSelector />
        <el-divider class="panel-divider" />
        <ConfigForm />
      </div>

      <footer class="panel-footer">
        <div class="tech-stack">
          <el-tag size="small" type="info" effect="plain">.NET 9</el-tag>
          <el-tag size="small" type="success" effect="plain">Vue 3</el-tag>
          <el-tag size="small" type="warning" effect="plain">Element+</el-tag>
        </div>
      </footer>
    </main>

    <!-- Preview Drawer -->
    <PreviewDrawer />
  </div>
</template>

<script setup lang="ts">
import { Box, View } from '@element-plus/icons-vue'
import { useConfigStore } from '@/stores/config'
import ConfigForm from '@/components/ConfigForm.vue'
import PresetSelector from '@/components/PresetSelector.vue'
import PreviewDrawer from '@/components/PreviewDrawer.vue'

const store = useConfigStore()
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: 40px 20px;
  background-color: #f8fafc;
}

/* Configuration Panel */
.config-panel {
  width: 100%;
  max-width: 800px;
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.panel-header {
  padding: 32px 40px 24px;
  background: white;
  border-bottom: 1px solid #f1f5f9;
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #0f172a;
}

.logo-icon {
  font-size: 28px;
  color: #4f46e5;
}

.logo h1 {
  font-size: 24px;
  margin: 0;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.subtitle {
  font-size: 14px;
  color: #64748b;
  margin: 0;
  padding-left: 40px;
}

.panel-content {
  flex: 1;
  overflow-y: auto;
  padding: 32px 40px;
}

.panel-divider {
  margin: 28px 0;
}

.panel-footer {
  padding: 20px 40px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: center;
  align-items: center;
}

.tech-stack {
  display: flex;
  gap: 8px;
}

/* Scrollbar Styling */
.panel-content::-webkit-scrollbar {
  width: 6px;
}
.panel-content::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}
.panel-content::-webkit-scrollbar-track {
  background: transparent;
}
</style>
