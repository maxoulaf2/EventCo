/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string
  /** SHA git du build, injecté par le Dockerfile (absent hors build Docker, cf. shared/lib/appVersion.ts). */
  readonly VITE_APP_VERSION?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
