import axios, { AxiosError } from 'axios'
import type { AxiosInstance } from 'axios'
import { ElMessage } from 'element-plus'
import type { ScaffoldConfig, ApiErrorResponse, ErrorCode, ScaffoldPreset, PreviewFileResponse, FileTreeNode } from '@/types'
import type { PackageReference } from '@/types/packages'

type ErrorMessageKey = ErrorCode | 'NetworkError' | 'Unknown'

const ERROR_MESSAGES: Record<ErrorMessageKey, string> = {
  None: '',
  ValidationError: '输入验证失败，请检查配置项',
  InvalidCombination: '配置组合无效，请调整选项',
  TemplateError: '模板生成失败，请稍后重试',
  NetworkError: '网络连接失败，请检查网络',
  Unknown: '发生未知错误，请重试'
}

function getErrorMessage(error: AxiosError<ApiErrorResponse>): string {
  if (!error.response) {
    return ERROR_MESSAGES.NetworkError
  }

  const data = error.response.data
  if (data?.error) {
    return data.error
  }

  const errorCode = data?.errorCode
  if (errorCode && ERROR_MESSAGES[errorCode]) {
    return ERROR_MESSAGES[errorCode]
  }

  return ERROR_MESSAGES.Unknown
}

const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 60000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    const message = getErrorMessage(error)
    ElMessage.error(message)
    return Promise.reject(error)
  }
)

// API 请求 DTO
interface GenerateScaffoldRequestDto {
  basic: {
    projectName: string
    namespace: string
  }
  backend: {
    architecture: string
    orm: string
    database: string
    cache: string
    swagger: boolean
    jwtAuth: boolean
    nugetPackages?: PackageReference[]
    unitTestFramework?: string
    integrationTestFramework?: string
  }
  frontend: {
    uiLibrary: string
    routerMode: string
    mockData: boolean
    npmPackages?: PackageReference[]
    unitTestFramework?: string
    e2eFramework?: string
  }
}

// 将扁平配置转换为嵌套结构
function toApiRequest(config: ScaffoldConfig): GenerateScaffoldRequestDto {
  return {
    basic: {
      projectName: config.projectName,
      namespace: config.namespace
    },
    backend: {
      architecture: config.architecture,
      orm: config.orm,
      database: config.database,
      cache: config.cache,
      swagger: config.enableSwagger,
      jwtAuth: config.enableJwtAuth,
      nugetPackages: config.nugetPackages,
      unitTestFramework: config.backendUnitTestFramework,
      integrationTestFramework: config.backendIntegrationTestFramework
    },
    frontend: {
      uiLibrary: config.uiLibrary,
      routerMode: config.routerMode,
      mockData: config.enableMockData,
      npmPackages: config.npmPackages,
      unitTestFramework: config.frontendUnitTestFramework,
      e2eFramework: config.frontendE2EFramework
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

export async function getPresets(): Promise<ScaffoldPreset[]> {
  const response = await api.get<{ presets: ScaffoldPreset[] }>('/v1/scaffolds/presets')
  return response.data.presets
}

export async function previewFile(config: ScaffoldConfig, outputPath: string): Promise<PreviewFileResponse> {
  const payload = {
    config: toApiRequest(config),
    outputPath
  }
  const response = await api.post<PreviewFileResponse>('/v1/scaffolds/preview-file', payload)
  return response.data
}

export async function getPreviewTree(config: ScaffoldConfig): Promise<{ tree: FileTreeNode[] }> {
  const payload = {
    config: toApiRequest(config)
  }
  const response = await api.post<{ tree: FileTreeNode[] }>('/v1/scaffolds/preview-tree', payload)
  return response.data
}

export default api
