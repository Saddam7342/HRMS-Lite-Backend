import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Select, TextArea, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

type BalanceRow = { leaveTypeId: string; leaveTypeName: string }
type LeaveReq = { id: string; status?: string }

export default function LeavesPage() {
  const { roles } = useAuth()
  const manager = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [balances, setBalances] = useState<BalanceRow[]>([])
  const [mine, setMine] = useState<unknown[]>([])
  const [pending, setPending] = useState<LeaveReq[]>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [leaveTypeId, setLeaveTypeId] = useState('')
  const [start, setStart] = useState(todayISODate())
  const [end, setEnd] = useState(todayISODate())
  const [reason, setReason] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    const [b, m] = await Promise.all([api.getLeaveBalances(new Date().getFullYear()), api.getMyLeaves()])
    if (b.success && b.data) {
      const arr = b.data as BalanceRow[]
      setBalances(arr)
    }
    if (m.success && m.data) setMine(m.data as unknown[])
    if (manager) {
      const p = await api.getPendingLeaves()
      if (p.success && p.data) setPending(p.data as LeaveReq[])
    } else setPending([])
    setLoading(false)
  }, [manager])

  useEffect(() => {
    void load()
  }, [load])

  async function submitLeave(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const lt = leaveTypeId || balances[0]?.leaveTypeId
    if (!lt) {
      setMsg({ type: 'err', text: 'No leave types available.' })
      return
    }
    const r = await api.createLeave({
      leaveTypeId: lt,
      startDate: `${start}T00:00:00Z`,
      endDate: `${end}T00:00:00Z`,
      reason: reason || null,
    })
    setMsg(r.success ? { type: 'ok', text: 'Leave request submitted.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function approve(id: string) {
    const r = await api.approveLeave(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const text = prompt('Reason (optional)') ?? ''
    const r = await api.rejectLeave(id, text || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function cancelMine(id: string) {
    const r = await api.cancelLeave(id)
    setMsg(r.success ? { type: 'ok', text: 'Cancelled.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle title="Leaves" subtitle="Balances, requests, and approvals" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <div className="grid gap-8 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 text-sm font-semibold">New request</h3>
          <form onSubmit={submitLeave} className="space-y-4">
            <Select label="Leave type" value={leaveTypeId} onChange={(e) => setLeaveTypeId(e.target.value)}>
              <option value="">Default (first balance)</option>
              {balances.map((b) => (
                <option key={b.leaveTypeId} value={b.leaveTypeId}>
                  {b.leaveTypeName}
                </option>
              ))}
            </Select>
            <Input type="date" label="Start" value={start} onChange={(e) => setStart(e.target.value)} />
            <Input type="date" label="End" value={end} onChange={(e) => setEnd(e.target.value)} />
            <TextArea label="Reason" value={reason} onChange={(e) => setReason(e.target.value)} rows={2} />
            <Btn type="submit">Submit</Btn>
          </form>
        </Card>

        <Card>
          <h3 className="mb-4 text-sm font-semibold">Balances ({new Date().getFullYear()})</h3>
          {loading ? (
            <Spinner />
          ) : (
            <pre className="max-h-56 overflow-auto text-xs">{JSON.stringify(balances, null, 2)}</pre>
          )}
        </Card>
      </div>

      <Card className="mt-8">
        <h3 className="mb-4 text-sm font-semibold">My requests</h3>
        <div className="space-y-2">
          {(mine as LeaveReq[]).map((row) => (
            <div
              key={row.id}
              className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-100 bg-slate-50/80 px-4 py-2 text-sm"
            >
              <code className="text-xs text-slate-600">{row.id}</code>
              <span className="text-slate-500">{row.status}</span>
              <Btn variant="secondary" onClick={() => void cancelMine(row.id)}>
                Cancel
              </Btn>
            </div>
          ))}
          {mine.length === 0 && <p className="text-sm text-slate-500">No requests yet.</p>}
        </div>
      </Card>

      {manager && (
        <Card className="mt-8">
          <h3 className="mb-4 text-sm font-semibold">Pending approvals</h3>
          <div className="space-y-3">
            {pending.map((row) => (
              <div
                key={row.id}
                className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-amber-100 bg-amber-50/50 px-4 py-3"
              >
                <code className="text-xs">{row.id}</code>
                <div className="flex gap-2">
                  <Btn onClick={() => void approve(row.id)}>Approve</Btn>
                  <Btn variant="danger" onClick={() => void reject(row.id)}>
                    Reject
                  </Btn>
                </div>
              </div>
            ))}
            {pending.length === 0 && <p className="text-sm text-slate-500">No pending items.</p>}
          </div>
        </Card>
      )}
    </div>
  )
}
