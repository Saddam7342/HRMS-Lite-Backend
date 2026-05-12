import { useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function AttendancePage() {
  const { roles } = useAuth()
  const isAdmin = hasRole(roles, 'Admin')
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)
  const [today, setToday] = useState<unknown>(null)
  const [rangeStart, setRangeStart] = useState(todayISODate())
  const [rangeEnd, setRangeEnd] = useState(todayISODate())
  const [myRange, setMyRange] = useState<unknown[]>([])
  const [summary, setSummary] = useState<unknown>(null)
  const [loading, setLoading] = useState(false)

  async function refreshToday() {
    const r = await api.getTodayAttendance()
    if (r.success) setToday(r.data)
  }

  useEffect(() => {
    void refreshToday()
  }, [])

  async function checkIn() {
    setMsg(null)
    const r = await api.checkIn(null)
    setMsg(r.success ? { type: 'ok', text: 'Checked in.' } : { type: 'err', text: apiErrorMessage(r) })
    await refreshToday()
  }

  async function checkOut() {
    setMsg(null)
    const r = await api.checkOut(null)
    setMsg(r.success ? { type: 'ok', text: 'Checked out.' } : { type: 'err', text: apiErrorMessage(r) })
    await refreshToday()
  }

  async function loadMyRange() {
    setLoading(true)
    const r = await api.getMyAttendance(`${rangeStart}T00:00:00Z`, `${rangeEnd}T23:59:59Z`)
    if (r.success && r.data) setMyRange(r.data as unknown[])
    setLoading(false)
  }

  async function loadSummary() {
    const r = await api.getAttendanceSummary(`${rangeStart}T00:00:00Z`, `${rangeEnd}T23:59:59Z`)
    if (r.success) setSummary(r.data)
  }

  return (
    <div>
      <PageTitle title="Attendance" subtitle="Check in/out and review your records" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <div className="mb-6 flex flex-wrap gap-3">
        <Btn onClick={checkIn}>Check in</Btn>
        <Btn variant="secondary" onClick={checkOut}>
          Check out
        </Btn>
      </div>

      <Card className="mb-8">
        <h3 className="mb-2 text-sm font-semibold">Today</h3>
        <pre className="max-h-48 overflow-auto rounded-xl bg-slate-50 p-4 text-xs text-slate-700">
          {today ? JSON.stringify(today, null, 2) : 'No record'}
        </pre>
      </Card>

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">My attendance (range)</h3>
        <div className="mb-4 flex flex-wrap items-end gap-4">
          <Input type="date" label="From" value={rangeStart} onChange={(e) => setRangeStart(e.target.value)} />
          <Input type="date" label="To" value={rangeEnd} onChange={(e) => setRangeEnd(e.target.value)} />
          <Btn onClick={() => void loadMyRange()}>Load</Btn>
          <Btn variant="secondary" onClick={() => void loadSummary()}>
            Summary
          </Btn>
        </div>
        {loading ? (
          <Spinner />
        ) : (
          <pre className="max-h-64 overflow-auto rounded-xl bg-slate-50 p-4 text-xs">
            {JSON.stringify(myRange, null, 2)}
          </pre>
        )}
        {summary != null && (
          <div className="mt-4">
            <div className="mb-2 text-xs font-medium text-slate-500">Summary</div>
            <pre className="max-h-40 overflow-auto rounded-xl bg-indigo-50/50 p-4 text-xs">
              {JSON.stringify(summary, null, 2)}
            </pre>
          </div>
        )}
      </Card>

      {isAdmin && (
        <Card>
          <p className="text-sm text-slate-600">
            Admin: use the API or Swagger for range/team attendance and overrides — full forms can be added here if you need
            them in the UI.
          </p>
        </Card>
      )}
    </div>
  )
}
