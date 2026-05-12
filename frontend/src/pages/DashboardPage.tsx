import { useEffect, useState } from 'react'
import { useAuth, hasRole } from '../context/AuthContext'
import * as api from '../lib/api'
import type { HrDashboardDto } from '../lib/types'
import { Card, PageTitle, Spinner } from '../components/Ui'
import { money } from '../lib/util'

export default function DashboardPage() {
  const { user, roles, loginHint } = useAuth()
  const [status, setStatus] = useState<Record<string, unknown> | null>(null)
  const [dash, setDash] = useState<HrDashboardDto | null>(null)
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
              if (!cancel && r.success && r.data) setDash(r.data)
            }),
          )
        }
        await Promise.all(jobs)
      } catch {
        if (!cancel) setErr('Failed to load dashboard')
      } finally {
        if (!cancel) setLoading(false)
      }
    })()
    return () => {
      cancel = true
    }
  }, [roles])

  const es = dash?.employeeSummary
  const ls = dash?.leaveSummary
  const xs = dash?.expenseSummary
  const ts = dash?.travelSummary
  const at = dash?.attendanceSummary

  return (
    <div>
      <PageTitle
        title={`Hello, ${user?.firstName ?? 'there'}`}
        subtitle="Admin overview — approvals and operational summaries."
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
            <div className="mt-2 font-mono text-sm text-slate-800">{String(status.status ?? '—')}</div>
          </Card>
        )}
      </div>

      {loading && (
        <div className="flex items-center gap-2 text-slate-500">
          <Spinner /> Loading…
        </div>
      )}
      {err && <p className="text-sm text-rose-600">{err}</p>}

      {dash && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <h3 className="mb-3 text-sm font-semibold text-slate-800">People</h3>
            <dl className="grid gap-2 text-sm">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Total employees</dt>
                <dd className="font-medium">{es?.totalEmployees ?? '—'}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Active</dt>
                <dd className="font-medium">{es?.activeEmployees ?? '—'}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">New hires (this month)</dt>
                <dd className="font-medium">{es?.newHiresThisMonth ?? '—'}</dd>
              </div>
            </dl>
            {es?.departmentDistribution?.length ? (
              <div className="mt-4 border-t border-slate-100 pt-3">
                <div className="mb-2 text-xs font-medium text-slate-500">By department</div>
                <ul className="space-y-1 text-sm">
                  {es.departmentDistribution.map((d) => (
                    <li key={d.departmentName} className="flex justify-between gap-2">
                      <span className="text-slate-700">{d.departmentName}</span>
                      <span className="text-slate-500">{d.count}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
          </Card>

          <Card>
            <h3 className="mb-3 text-sm font-semibold text-slate-800">Leaves</h3>
            <dl className="grid gap-2 text-sm">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Pending</dt>
                <dd className="font-medium text-amber-700">{ls?.pendingCount ?? '—'}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Approved</dt>
                <dd className="font-medium">{ls?.approvedCount ?? '—'}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Rejected</dt>
                <dd className="font-medium">{ls?.rejectedCount ?? '—'}</dd>
              </div>
            </dl>
          </Card>

          <Card>
            <h3 className="mb-3 text-sm font-semibold text-slate-800">Expenses</h3>
            <dl className="grid gap-2 text-sm">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Pending</dt>
                <dd className="font-medium">{money(xs?.pendingAmount)}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Approved</dt>
                <dd className="font-medium">{money(xs?.approvedAmount)}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Total claimed</dt>
                <dd className="font-medium">{money(xs?.totalClaimed)}</dd>
              </div>
            </dl>
          </Card>

          <Card>
            <h3 className="mb-3 text-sm font-semibold text-slate-800">Travel</h3>
            <dl className="grid gap-2 text-sm">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Pending</dt>
                <dd className="font-medium">{ts?.pendingCount ?? '—'}</dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Approved</dt>
                <dd className="font-medium">{ts?.approvedCount ?? '—'}</dd>
              </div>
            </dl>
            {ts?.destinationDistribution?.length ? (
              <div className="mt-4 border-t border-slate-100 pt-3">
                <div className="mb-2 text-xs font-medium text-slate-500">Top destinations</div>
                <ul className="space-y-1 text-sm">
                  {ts.destinationDistribution.map((d) => (
                    <li key={d.destination} className="flex justify-between gap-2">
                      <span className="text-slate-700">{d.destination}</span>
                      <span className="text-slate-500">{d.count}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
          </Card>

          <Card className="lg:col-span-2">
            <h3 className="mb-3 text-sm font-semibold text-slate-800">Attendance</h3>
            <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <dt className="text-slate-500">Avg. hours</dt>
                <dd className="mt-1 font-medium">{at?.averageWorkingHours?.toFixed?.(1) ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Late arrivals</dt>
                <dd className="mt-1 font-medium">{at?.lateArrivalsCount ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Missing checkouts</dt>
                <dd className="mt-1 font-medium">{at?.missingCheckoutsCount ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Presence ratio</dt>
                <dd className="mt-1 font-medium">
                  {at?.presenceRatio != null ? `${(at.presenceRatio * 100).toFixed(0)}%` : '—'}
                </dd>
              </div>
            </dl>
          </Card>
        </div>
      )}
    </div>
  )
}
