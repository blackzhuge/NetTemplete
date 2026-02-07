import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

// 后端 API 地址：优先使用环境变量，默认 5241（与 dev.sh 保持一致）
const apiTarget = process.env.VITE_API_TARGET || 'http://localhost:5241'

export default defineConfig({
  plugins: [
    vue(),
    AutoImport({
      resolvers: [ElementPlusResolver()],
      imports: ['vue', 'vue-router', 'pinia'],
      dts: 'src/auto-imports.d.ts'
    }),
    Components({
      resolvers: [ElementPlusResolver()],
      dts: 'src/components.d.ts'
    })
  ],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
        configure: (proxy) => {
          proxy.on('error', (err) => {
            console.error('\x1b[31m[Proxy Error]\x1b[0m', err.message)
            console.error('\x1b[33m[Hint]\x1b[0m 后端服务可能未启动，请运行: scripts/dev.sh')
          })
        }
      }
    }
  }
})
