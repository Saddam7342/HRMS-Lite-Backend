import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { AttendanceDto, AttendanceListDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, formatDate, formatTimeSpan, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function AttendancePage() {
  const { roles } = useAuth()
  const isAdmin = hasRole(roles, 'Admin')
  const canTeam = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [tab, setTab] = useState<'team' | 'range'>('team')
  const [teamDate, setTeamDate] = useState(todayISODate())
  const [teamRows, setTeamRows] = useState<AttendanceListDto[]>([])
  const [rangeStart, setRangeStart] = useState(todayISODate())
  const [rangeEnd, setRangeEnd] = useState(todayISODate())
  const [rangeRows, setRangeRows] = useState<AttendanceDto[]>([])
  const [loading, setLoading] = useState(false)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  useEffect(() => {
    if (!canTeam) setTab('range')
  }, [canTeam])

  const loadTeam = useCallback(async () => {
    if (!canTeam) return
    setLoading(true)
    setMsg(null)
    const day = `${teamDate}T12:00:00Z`
    const r = await api.getTeamAttendance(day)
    if (r.success && r.data) setTeamRows(r.data)
    else setMsg({ type: 'err', text: apiErrorMessage(r) })
    setLoading(false)
  }, [canTeam, teamDate])

  const loadRange = useCallback(async () => {
    if (!isAdmin) return
    setLoading(true)
    setMsg(null)
    const start = `${rangeStart}T00:00:00Z`
    const end = `${rangeEnd}T23:59:59Z`
    const r = await api.getAttendanceRange(start, end)
    if (r.success && r.data) setRangeRows(r.data)
    else setMsg({ type: 'err', text: apiErrorMessage(r) })
    setLoading(false)
  }, [isAdmin, rangeStart, rangeEnd])

  useEffect(() => {
    if (tab === 'team' && canTeam) void loadTeam()
  }, [tab, canTeam, loadTeam])

  return (
    <div>
      <PageTitle
        title="Attendance"
        subtitle="View check-in and check-out records across the organization (read-only on web)."
      />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {!canTeam && !isAdmin && (
        <Alert type="info">You do not have permission to view team or organization attendance.</Alert>
      )}

      <div className="mb-6 flex flex-wrap gap-2">
        {canTeam && (
          <Btn variant={tab === 'team' ? 'primary' : 'secondary'} onClick={() => setTab('team')}>
            By date (team)
          </Btn>
        )}
        {isAdmin && (
          <Btn variant={tab === 'range' ? 'primary' : 'secondary'} onClick={() => setTab('range')}>
            Date range (organization)
          </Btn>
        )}
      </div>

      {tab === 'team' && canTeam && (
        <Card className="mb-8">
          <div className="mb-4 flex flex-wrap items-end gap-4">
            <Input type="date" label="Date" value={teamDate} onChange={(e) => setTeamDate(e.target.value)} />
            <Btn onClick={() => void loadTeam()}>Load</Btn>
          </div>
          {loading ? (
            <Spinner />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-200 text-slate-500">
                    <th className="pb-3 font-medium">Employee</th>
                    <th className="pb-3 font-medium">Date</th>
                    <th className="pb-3 font-medium">Check in</th>
                    <th className="pb-3 font-medium">Check out</th>
                    <th className="pb-3 font-medium">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {teamRows.map((row) => (
                    <tr key={row.id} className="border-b border-slate-100">
                      <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                      <td className="py-3 text-slate-600">{formatDate(row.date)}</td>
                      <td className="py-3">{formatTimeSpan(row.checkInTime)}</td>
                      <td className="py-3">{formatTimeSpan(row.checkOutTime)}</td>
                      <td className="py-3">{row.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {teamRows.length === 0 && <p className="mt-4 text-sm text-slate-500">No rows for this date.</p>}
            </div>
          )}
        </Card>
      )}

      {tab === 'range' && isAdmin && (
        <Card>
          <div className="mb-4 flex flex-wrap items-end gap-4">
            <Input type="date" label="From" value={rangeStart} onChange={(e) => setRangeStart(e.target.value)} />
            <Input type="date" label="To" value={rangeEnd} onChange={(e) => setRangeEnd(e.target.value)} />
            <Btn onClick={() => void loadRange()}>Load</Btn>
          </div>
          {loading ? (
            <Spinner />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-200 text-slate-500">
                    <th className="pb-3 font-medium">Employee</th>
                    <th className="pb-3 font-medium">Date</th>
                    <th className="pb-3 font-medium">Check in</th>
                    <th className="pb-3 font-medium">Check out</th>
                    <th className="pb-3 font-medium">Hours</th>
                    <th className="pb-3 font-medium">Status</th>
                    <th className="pb-3 font-medium">Late</th>
                  </tr>
                </thead>
                <tbody>
                  {rangeRows.map((row) => (
                    <tr key={row.id} className="border-b border-slate-100">
                      <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                      <td className="py-3 text-slate-600">{formatDate(row.date)}</td>
                      <td className="py-3">{formatTimeSpan(row.checkInTime)}</td>
                      <td className="py-3">{formatTimeSpan(row.checkOutTime)}</td>
                      <td className="py-3">{row.totalHours != null ? Number(row.totalHours).toFixed(2) : '—'}</td>
                      <td className="py-3">{row.status}</td>
                      <td className="py-3">{row.isLate ? 'Yes' : 'No'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {rangeRows.length === 0 && <p className="mt-4 text-sm text-slate-500">No records in this range.</p>}
            </div>
          )}
        </Card>
      )}
    </div>
  )
}
