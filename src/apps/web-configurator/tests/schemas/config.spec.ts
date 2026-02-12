import { describe, it, expect } from 'vitest'
import { scaffoldConfigSchema } from '@/schemas/config'

describe('scaffoldConfigSchema - test framework fields', () => {
  const validBase = {
    projectName: 'MyApp',
    namespace: 'MyApp',
    architecture: 'CleanArchitecture',
    orm: 'SqlSugar',
    database: 'SQLite',
    cache: 'None',
    enableSwagger: true,
    enableJwtAuth: false,
    uiLibrary: 'ElementPlus',
    routerMode: 'Hash',
    enableMockData: false,
    backendUnitTestFramework: 'None',
    backendIntegrationTestFramework: 'None',
    frontendUnitTestFramework: 'None',
    frontendE2EFramework: 'None'
  }

  it('should accept valid test framework values', () => {
    const result = scaffoldConfigSchema.safeParse({
      ...validBase,
      backendUnitTestFramework: 'xUnit',
      backendIntegrationTestFramework: 'xUnit',
      frontendUnitTestFramework: 'Vitest',
      frontendE2EFramework: 'Playwright'
    })
    expect(result.success).toBe(true)
  })

  it('should reject invalid backend unit test framework', () => {
    const result = scaffoldConfigSchema.safeParse({
      ...validBase,
      backendUnitTestFramework: 'Jest'
    })
    expect(result.success).toBe(false)
  })

  it('should reject invalid frontend E2E framework', () => {
    const result = scaffoldConfigSchema.safeParse({
      ...validBase,
      frontendE2EFramework: 'Selenium'
    })
    expect(result.success).toBe(false)
  })

  it('should accept None for all test fields', () => {
    const result = scaffoldConfigSchema.safeParse(validBase)
    expect(result.success).toBe(true)
  })
})
