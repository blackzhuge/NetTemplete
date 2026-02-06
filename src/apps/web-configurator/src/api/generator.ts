import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { ElMessage } from 'element-plus'
import type { ScaffoldConfig } from '@/types'

const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.error || '请求失败'
    ElMessage.error(message)
    return Promise.reject(error)
  }
)

// 将扁平配置转换为嵌套结构
function toApiRequest(config: ScaffoldConfig) {
  return {
    basic: {
      projectName: config.projectName,
      namespace: config.namespace
    },
    backend: {
      database: config.database,
      cache: config.cache,
      swagger: config.enableSwagger,
      jwtAuth: config.enableJwtAuth
    },
    frontend: {
      routerMode: config.routerMode.toLowerCase(),
      mockData: config.enableMockData
    }
  }
}

export async function generateScaffold(config: ScaffoldConfig): Promise<Blob> {
  const payload = toApiRequest(config)
  const response = await api.post('/v1/scaffolds/generate-zip', payload, {
    responseType: 'blob'
  })
  return response.data
}

export async function healthCheck(): Promise<boolean> {
  try {
    await api.get('/health')
    return true
  } catch {
    return false
  }
}

export default api
