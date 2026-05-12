import type { ApiResponse } from './types'

export function apiErrorMessage(r: ApiResponse<unknown> | null | undefined): string {
  if (!r) return 'Request failed'
  if (r.errors?.length) return r.errors.join('; ')
  return r.message ?? 'Request failed'
}

export function todayISODate() {
  return new Date().toISOString().slice(0, 10)
}

/** ISO date string → local short date */
export function formatDate(iso: string | null | undefined) {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

/** ISO datetime → local date + time */
export function formatDateTime(iso: string | null | undefined) {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  return d.toLocaleString(undefined, { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}

/** API TimeSpan as "HH:mm:ss" or similar → readable time */
export function formatTimeSpan(ts: string | null | undefined) {
  if (!ts) return '—'
  const parts = ts.split(':')
  if (parts.length >= 2) {
    const h = parseInt(parts[0], 10)
    const m = parseInt(parts[1], 10)
    if (!Number.isNaN(h) && !Number.isNaN(m)) return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`
  }
  return ts
}

export function money(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}
