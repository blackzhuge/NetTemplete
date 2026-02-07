/** 包信息 - 用于搜索结果展示 */
export interface PackageInfo {
  name: string
  version: string
  description: string
  iconUrl?: string
}

/** 包引用 - 用于存储用户选择的包 */
export interface PackageReference {
  name: string
  version: string
  source?: string
}

/** 包源配置 */
export interface PackageSource {
  name: string
  url: string
  isDefault: boolean
}

/** 包搜索响应 */
export interface PackageSearchResponse {
  items: PackageInfo[]
  totalCount: number
}

/** 包版本响应 */
export interface PackageVersionsResponse {
  versions: string[]
}

/** 包管理器类型 */
export type PackageManagerType = 'nuget' | 'npm'

/** 预置包源 */
export const NUGET_SOURCES: PackageSource[] = [
  { name: 'nuget.org', url: 'https://api.nuget.org/v3/index.json', isDefault: true }
]

export const NPM_SOURCES: PackageSource[] = [
  { name: 'npmjs.org', url: 'https://registry.npmjs.org/', isDefault: true },
  { name: '淘宝镜像', url: 'https://registry.npmmirror.com/', isDefault: false }
]
