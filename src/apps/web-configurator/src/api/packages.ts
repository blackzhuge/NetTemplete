import api from './generator'
import type { PackageSearchResponse, PackageVersionsResponse, PackageManagerType } from '@/types/packages'

/**
 * 搜索 NuGet 包
 */
export async function searchNugetPackages(
  query: string,
  source?: string,
  take = 20
): Promise<PackageSearchResponse> {
  const params = new URLSearchParams({ q: query, take: String(take) })
  if (source) params.set('source', source)

  const response = await api.get<PackageSearchResponse>(
    `/v1/packages/nuget/search?${params}`
  )
  return response.data
}

/**
 * 搜索 npm 包
 */
export async function searchNpmPackages(
  query: string,
  source?: string,
  take = 20
): Promise<PackageSearchResponse> {
  const params = new URLSearchParams({ q: query, take: String(take) })
  if (source) params.set('source', source)

  const response = await api.get<PackageSearchResponse>(
    `/v1/packages/npm/search?${params}`
  )
  return response.data
}

/**
 * 获取 NuGet 包版本列表
 */
export async function getNugetVersions(
  packageId: string,
  source?: string
): Promise<string[]> {
  const params = source ? `?source=${encodeURIComponent(source)}` : ''
  const response = await api.get<PackageVersionsResponse>(
    `/v1/packages/nuget/${encodeURIComponent(packageId)}/versions${params}`
  )
  return response.data.versions
}

/**
 * 获取 npm 包版本列表
 */
export async function getNpmVersions(
  packageName: string,
  source?: string
): Promise<string[]> {
  const params = source ? `?source=${encodeURIComponent(source)}` : ''
  const response = await api.get<PackageVersionsResponse>(
    `/v1/packages/npm/${encodeURIComponent(packageName)}/versions${params}`
  )
  return response.data.versions
}

/**
 * 通用搜索函数
 */
export async function searchPackages(
  type: PackageManagerType,
  query: string,
  source?: string,
  take = 20
): Promise<PackageSearchResponse> {
  return type === 'nuget'
    ? searchNugetPackages(query, source, take)
    : searchNpmPackages(query, source, take)
}

/**
 * 通用获取版本函数
 */
export async function getPackageVersions(
  type: PackageManagerType,
  packageId: string,
  source?: string
): Promise<string[]> {
  return type === 'nuget'
    ? getNugetVersions(packageId, source)
    : getNpmVersions(packageId, source)
}
