import { useEffect, useState } from 'react'
import { useAuth, hasRole } from '../context/AuthContext'
import * as api from '../lib/api'
import { Card, PageTitle, Spinner } from '../components/Ui'
export default function DashboardPage() {
  const { user, roles, loginHint } = useAuth()
  const [status, setStatus] = useState<Record<string, unknown> | null>(null)
  const [dash, setDash] = useState<unknown>(null)
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState<string | null>(null)

  useEffect(() => {
    let cancel = false
    ;(async () => {
      setLoading(true)
      setErr(null)
      try {
        const jobs: Promise<void>[] = []
        if (hasRole(roles, 'Admin')) {
          jobs.push(
            api.getSystemStatus().then((r) => {
              if (!cancel && r.success) setStatus((r.data as Record<string, unknown>) ?? null)
            }),
          )
        }
        if (hasRole(roles, 'Admin') || hasRole(roles, 'Manager')) {
          jobs.push(
            api.getHrDashboard().then((r) => {
              if (!cancel && r.success) setDash(r.data)
            }),
          )
        }
        await Promise.all(jobs)
      } catch {
        if (!cancel) setErr('Failed to load dashboard widgets')
      } finally {
        if (!cancel) setLoading(false)
      }
    })()
    return () => {
      cancel = true
    }
  }, [roles])

  return (
    <div>
      <PageTitle
        title={`Hello, ${user?.firstName ?? 'there'}`}
        subtitle="Here’s a snapshot of your workspace."
      />

      <div className="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Card className="border-indigo-100 bg-gradient-to-br from-white to-indigo-50/40">
          <div className="text-xs font-medium uppercase tracking-wide text-indigo-600">Signed in as</div>
          <div className="mt-2 text-lg font-semibold text-slate-900">{user?.email}</div>
          <div className="mt-1 text-sm text-slate-500">{(loginHint?.roles ?? roles).join(', ')}</div>
        </Card>
        {hasRole(roles, 'Admin') && status && (
          <Card>
            <div className="text-xs font-medium uppercase tracking-wide text-slate-500">System</div>
            <div className="mt-2 font-mono text-sm text-slate-800">{String((status as { status?: string }).status ?? '—')}</div>
            <div className="mt-1 text-xs text-slate-500">From /system/status</div>
          </Card>
        )}
      </div>

      {loading && (
        <div className="flex items-center gap-2 text-slate-500">
          <Spinner /> Loading…
        </div>
      )}
      {err && <p className="text-sm text-rose-600">{err}</p>}

      {dash != null && (
        <Card>
          <div className="mb-3 text-sm font-semibold text-slate-800">HR dashboard summary</div>
          <pre className="max-h-80 overflow-auto rounded-xl bg-slate-50 p-4 text-xs text-slate-700">
            {JSON.stringify(dash, null, 2)}
          </pre>
        </Card>
      )}
    </div>
  )
}
