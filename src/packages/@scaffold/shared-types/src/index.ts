export type DatabaseProvider = 'SQLite' | 'MySQL' | 'SQLServer';
export type CacheProvider = 'None' | 'MemoryCache' | 'Redis';
export type RouterMode = 'hash' | 'history';

export interface BasicOptions {
  projectName: string;
  namespace: string;
}

export interface BackendOptions {
  database: DatabaseProvider;
  cache: CacheProvider;
  swagger: boolean;
  jwtAuth: boolean;
}

export interface FrontendOptions {
  routerMode: RouterMode;
  mockData: boolean;
}

export interface ScaffoldConfig {
  basic: BasicOptions;
  backend: BackendOptions;
  frontend: FrontendOptions;
}

export const defaultConfig: ScaffoldConfig = {
  basic: {
    projectName: 'MyApp',
    namespace: 'MyApp',
  },
  backend: {
    database: 'SQLite',
    cache: 'None',
    swagger: true,
    jwtAuth: true,
  },
  frontend: {
    routerMode: 'hash',
    mockData: false,
  },
};
