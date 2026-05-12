import { useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { todayISODate } from '../lib/util'

export default function ReportsPage() {
  const [start, setStart] = useState(todayISODate())
  const [end, setEnd] = useState(todayISODate())
  const [hr, setHr] = useState<unknown>(null)
  const [leaves, setLeaves] = useState<unknown>(null)
  const [expenses, setExpenses] = useState<unknown>(null)
  const [attendance, setAttendance] = useState<unknown>(null)
  const [loading, setLoading] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  async function loadAll() {
    setLoading(true)
    setErr(null)
    try {
      const s = start ? `${start}T00:00:00Z` : null
      const e = end ? `${end}T23:59:59Z` : null
      const [a, b, c, d] = await Promise.all([
        api.getHrDashboard(),
        api.getLeaveReport(s, e),
        api.getExpenseReport(s, e),
        api.getAttendanceReport(s, e),
      ])
      if (a.success) setHr(a.data)
      if (b.success) setLeaves(b.data)
      if (c.success) setExpenses(c.data)
      if (d.success) setAttendance(d.data)
      if (!a.success) setErr('One or more report calls failed — check roles (Admin/Manager).')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <PageTitle title="Reports" subtitle="Analytics summaries" />
      <Card className="mb-6">
        <div className="mb-4 flex flex-wrap items-end gap-4">
          <Input type="date" label="Start" value={start} onChange={(e) => setStart(e.target.value)} />
          <Input type="date" label="End" value={end} onChange={(e) => setEnd(e.target.value)} />
          <Btn onClick={() => void loadAll()}>Load reports</Btn>
        </div>
        {err && <Alert type="err">{err}</Alert>}
        {loading && (
          <div className="flex items-center gap-2 text-slate-500">
            <Spinner /> Loading…
          </div>
        )}
      </Card>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-2 text-sm font-semibold">HR dashboard</h3>
          <pre className="max-h-64 overflow-auto text-xs">{JSON.stringify(hr, null, 2)}</pre>
        </Card>
        <Card>
          <h3 className="mb-2 text-sm font-semibold">Leaves</h3>
          <pre className="max-h-64 overflow-auto text-xs">{JSON.stringify(leaves, null, 2)}</pre>
        </Card>
        <Card>
          <h3 className="mb-2 text-sm font-semibold">Expenses</h3>
          <pre className="max-h-64 overflow-auto text-xs">{JSON.stringify(expenses, null, 2)}</pre>
        </Card>
        <Card>
          <h3 className="mb-2 text-sm font-semibold">Attendance</h3>
          <pre className="max-h-64 overflow-auto text-xs">{JSON.stringify(attendance, null, 2)}</pre>
        </Card>
      </div>
    </div>
  )
}
