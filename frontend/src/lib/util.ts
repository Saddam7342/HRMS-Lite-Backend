import type { ApiResponse } from './types'

export function apiErrorMessage(r: ApiResponse<unknown> | null | undefined): string {
  if (!r) return 'Request failed'
  if (r.errors?.length) return r.errors.join('; ')
  return r.message ?? 'Request failed'
}

export function todayISODate() {
  return new Date().toISOString().slice(0, 10)
}
