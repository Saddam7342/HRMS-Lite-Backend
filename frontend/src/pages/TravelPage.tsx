import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { TeamTravelScheduleDto, TravelRequestDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, formatDate, money, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function TravelPage() {
  const { roles } = useAuth()
  const approver = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [pending, setPending] = useState<TravelRequestDto[]>([])
  const [schedule, setSchedule] = useState<TeamTravelScheduleDto[]>([])
  const [allTravel, setAllTravel] = useState<TravelRequestDto[]>([])
  const [tab, setTab] = useState<'pending' | 'schedule' | 'org'>('pending')
  const [schedStart, setSchedStart] = useState(todayISODate())
  const [schedEnd, setSchedEnd] = useState(todayISODate())
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const loadPending = useCallback(async () => {
    if (!approver) return
    const p = await api.getPendingTravel()
    if (p.success && p.data) setPending(p.data)
  }, [approver])

  const loadSchedule = useCallback(async () => {
    if (!approver) return
    const s = await api.getTeamTravelSchedule(`${schedStart}T00:00:00Z`, `${schedEnd}T23:59:59Z`)
    if (s.success && s.data) setSchedule(s.data)
  }, [approver, schedStart, schedEnd])

  const load = useCallback(async () => {
    setLoading(true)
    await loadPending()
    await loadSchedule()
    if (hasRole(roles, 'Admin')) {
      const a = await api.getAllTravel()
      if (a.success && a.data) setAllTravel(a.data)
    }
    setLoading(false)
  }, [roles, loadPending, loadSchedule])

  useEffect(() => {
    void load()
  }, [load])

  async function approve(id: string) {
    const r = await api.approveTravel(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const reason = window.prompt('Reason for rejection?') ?? ''
    const r = await api.rejectTravel(id, reason || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle
        title="Travel"
        subtitle="Approve travel requests and view the team schedule (employee requests go through mobile)."
      />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {!approver && <Alert type="info">Travel approvals are available to Admin and Manager roles.</Alert>}

      {approver && (
        <div className="mb-6 flex gap-2">
          <Btn variant={tab === 'pending' ? 'primary' : 'secondary'} onClick={() => setTab('pending')}>
            Pending
          </Btn>
          <Btn variant={tab === 'schedule' ? 'primary' : 'secondary'} onClick={() => setTab('schedule')}>
            Schedule
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
                        <th className="pb-3 font-medium">Destination</th>
                        <th className="pb-3 font-medium">From</th>
                        <th className="pb-3 font-medium">To</th>
                        <th className="pb-3 font-medium">Budget</th>
                        <th className="pb-3 font-medium">Purpose</th>
                        <th className="pb-3 font-medium text-right"> </th>
                      </tr>
                    </thead>
                    <tbody>
                      {pending.map((row) => (
                        <tr key={row.id} className="border-b border-slate-100">
                          <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                          <td className="py-3">{row.destination}</td>
                          <td className="py-3 text-slate-600">{formatDate(row.fromDate)}</td>
                          <td className="py-3 text-slate-600">{formatDate(row.toDate)}</td>
                          <td className="py-3">{row.estimatedBudget != null ? money(row.estimatedBudget) : '—'}</td>
                          <td className="max-w-xs truncate py-3 text-slate-600" title={row.purpose}>
                            {row.purpose}
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

          {tab === 'schedule' && (
            <Card>
              <h3 className="mb-4 text-sm font-semibold text-slate-800">Team travel schedule</h3>
              <div className="mb-4 flex flex-wrap items-end gap-4">
                <span className="text-xs text-slate-500">Range:</span>
                <Input type="date" label="From" value={schedStart} onChange={(e) => setSchedStart(e.target.value)} />
                <Input type="date" label="To" value={schedEnd} onChange={(e) => setSchedEnd(e.target.value)} />
                <Btn onClick={() => void loadSchedule()}>Load</Btn>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-slate-500">
                      <th className="pb-3 font-medium">Employee</th>
                      <th className="pb-3 font-medium">Destination</th>
                      <th className="pb-3 font-medium">From</th>
                      <th className="pb-3 font-medium">To</th>
                      <th className="pb-3 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {schedule.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium">{row.employeeName}</td>
                        <td className="py-3">{row.destination}</td>
                        <td className="py-3">{formatDate(row.fromDate)}</td>
                        <td className="py-3">{formatDate(row.toDate)}</td>
                        <td className="py-3">{row.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {schedule.length === 0 && !loading && (
                  <p className="mt-4 text-sm text-slate-500">No travel in this range.</p>
                )}
              </div>
            </Card>
          )}

          {tab === 'org' && hasRole(roles, 'Admin') && (
            <Card>
              <h3 className="mb-4 text-sm font-semibold text-slate-800">All organization travel</h3>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-slate-500">
                      <th className="pb-3 font-medium">Employee</th>
                      <th className="pb-3 font-medium">Destination</th>
                      <th className="pb-3 font-medium">From</th>
                      <th className="pb-3 font-medium">To</th>
                      <th className="pb-3 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {allTravel.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                        <td className="py-3">{row.destination}</td>
                        <td className="py-3">{formatDate(row.fromDate)}</td>
                        <td className="py-3">{formatDate(row.toDate)}</td>
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
