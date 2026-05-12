import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { LeaveCalendarDto, LeaveRequestDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, formatDate, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function LeavesPage() {
  const { roles } = useAuth()
  const approver = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [pending, setPending] = useState<LeaveRequestDto[]>([])
  const [calendar, setCalendar] = useState<LeaveCalendarDto[]>([])
  const [allLeaves, setAllLeaves] = useState<LeaveRequestDto[]>([])
  const [tab, setTab] = useState<'pending' | 'calendar' | 'org'>('pending')
  const [calStart, setCalStart] = useState(todayISODate())
  const [calEnd, setCalEnd] = useState(todayISODate())
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const loadPending = useCallback(async () => {
    if (!approver) return
    const p = await api.getPendingLeaves()
    if (p.success && p.data) setPending(p.data)
  }, [approver])

  const load = useCallback(async () => {
    setLoading(true)
    await loadPending()
    if (approver) {
      const c = await api.getTeamLeaveCalendar(`${calStart}T00:00:00Z`, `${calEnd}T23:59:59Z`)
      if (c.success && c.data) setCalendar(c.data)
      
      if (hasRole(roles, 'Admin')) {
        const a = await api.getAllLeaves()
        if (a.success && a.data) setAllLeaves(a.data)
      }
    } else setCalendar([])
    setLoading(false)
  }, [approver, roles, loadPending, calStart, calEnd])

  useEffect(() => {
    void load()
  }, [load])

  async function approve(id: string) {
    const r = await api.approveLeave(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const text = window.prompt('Reason for rejection (optional)') ?? ''
    const r = await api.rejectLeave(id, text || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reloadCalendar() {
    setLoading(true)
    const c = await api.getTeamLeaveCalendar(`${calStart}T00:00:00Z`, `${calEnd}T23:59:59Z`)
    if (c.success && c.data) setCalendar(c.data)
    setLoading(false)
  }

  return (
    <div>
      <PageTitle
        title="Leaves"
        subtitle="Approve or reject leave requests and browse the team leave calendar (employee self-service is on mobile)."
      />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {!approver && (
        <Alert type="info">Leave approvals are available to Admin and Manager roles.</Alert>
      )}
      
      {approver && (
        <div className="mb-6 flex gap-2">
          <Btn variant={tab === 'pending' ? 'primary' : 'secondary'} onClick={() => setTab('pending')}>
            Pending
          </Btn>
          <Btn variant={tab === 'calendar' ? 'primary' : 'secondary'} onClick={() => setTab('calendar')}>
            Calendar
          </Btn>
          {hasRole(roles, 'Admin') && (
            <Btn variant={tab === 'org' ? 'primary' : 'secondary'} onClick={() => setTab('org')}>
              Organization
            </Btn>
          )}
        </div>
      )}

      {approver && (
        <>
          {tab === 'pending' && (
            <Card className="mb-8">
              <h3 className="mb-4 text-sm font-semibold text-slate-800">Pending approvals</h3>
              {loading ? (
                <Spinner />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm">
                    <thead>
                      <tr className="border-b border-slate-200 text-slate-500">
                        <th className="pb-3 font-medium">Employee</th>
                        <th className="pb-3 font-medium">Type</th>
                        <th className="pb-3 font-medium">Start</th>
                        <th className="pb-3 font-medium">End</th>
                        <th className="pb-3 font-medium">Days</th>
                        <th className="pb-3 font-medium">Reason</th>
                        <th className="pb-3 font-medium text-right"> </th>
                      </tr>
                    </thead>
                    <tbody>
                      {pending.map((row) => (
                        <tr key={row.id} className="border-b border-slate-100">
                          <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                          <td className="py-3">{row.leaveTypeName}</td>
                          <td className="py-3 text-slate-600">{formatDate(row.startDate)}</td>
                          <td className="py-3 text-slate-600">{formatDate(row.endDate)}</td>
                          <td className="py-3">{row.totalDays}</td>
                          <td className="max-w-xs truncate py-3 text-slate-600" title={row.reason ?? ''}>
                            {row.reason ?? '—'}
                          </td>
                          <td className="py-3 text-right">
                            <div className="flex justify-end gap-2">
                              <Btn onClick={() => void approve(row.id)}>Approve</Btn>
                              <Btn variant="danger" onClick={() => void reject(row.id)}>
                                Reject
                              </Btn>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                  {pending.length === 0 && <p className="mt-4 text-sm text-slate-500">No pending requests.</p>}
                </div>
              )}
            </Card>
          )}

          {tab === 'calendar' && (
            <Card>
              <h3 className="mb-4 text-sm font-semibold text-slate-800">Team leave calendar</h3>
              <div className="mb-4 flex flex-wrap items-end gap-4">
                <Input type="date" label="From" value={calStart} onChange={(e) => setCalStart(e.target.value)} />
                <Input type="date" label="To" value={calEnd} onChange={(e) => setCalEnd(e.target.value)} />
                <Btn onClick={() => void reloadCalendar()}>Load</Btn>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-slate-500">
                      <th className="pb-3 font-medium">Employee</th>
                      <th className="pb-3 font-medium">Type</th>
                      <th className="pb-3 font-medium">Start</th>
                      <th className="pb-3 font-medium">End</th>
                      <th className="pb-3 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {calendar.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium">{row.employeeName}</td>
                        <td className="py-3">{row.leaveTypeName}</td>
                        <td className="py-3">{formatDate(row.startDate)}</td>
                        <td className="py-3">{formatDate(row.endDate)}</td>
                        <td className="py-3">{row.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {calendar.length === 0 && !loading && (
                  <p className="mt-4 text-sm text-slate-500">No entries in this range.</p>
                )}
              </div>
            </Card>
          )}

          {tab === 'org' && hasRole(roles, 'Admin') && (
            <Card>
              <h3 className="mb-4 text-sm font-semibold text-slate-800">All organization leaves</h3>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-slate-500">
                      <th className="pb-3 font-medium">Employee</th>
                      <th className="pb-3 font-medium">Type</th>
                      <th className="pb-3 font-medium">Start</th>
                      <th className="pb-3 font-medium">End</th>
                      <th className="pb-3 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {allLeaves.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                        <td className="py-3">{row.leaveTypeName}</td>
                        <td className="py-3">{formatDate(row.startDate)}</td>
                        <td className="py-3">{formatDate(row.endDate)}</td>
                        <td className="py-3">{row.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
        </>
      )}
    </div>
  )
}
