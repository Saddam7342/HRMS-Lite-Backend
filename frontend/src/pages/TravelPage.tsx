import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, TextArea, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function TravelPage() {
  const { roles } = useAuth()
  const manager = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [mine, setMine] = useState<unknown[]>([])
  const [pending, setPending] = useState<Array<{ id: string }>>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [destination, setDestination] = useState('')
  const [purpose, setPurpose] = useState('')
  const [fromDate, setFromDate] = useState(todayISODate())
  const [toDate, setToDate] = useState(todayISODate())
  const [budget, setBudget] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    const m = await api.getMyTravel()
    if (m.success && m.data) setMine(m.data as unknown[])
    if (manager) {
      const p = await api.getPendingTravel()
      if (p.success && p.data) setPending(p.data as { id: string }[])
    } else setPending([])
    setLoading(false)
  }, [manager])

  useEffect(() => {
    void load()
  }, [load])

  async function createReq(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const body: Record<string, unknown> = {
      destination,
      purpose,
      fromDate: `${fromDate}T00:00:00Z`,
      toDate: `${toDate}T00:00:00Z`,
    }
    if (budget) body.estimatedBudget = Number(budget)
    const r = await api.createTravel(body)
    setMsg(r.success ? { type: 'ok', text: 'Travel request created.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function approve(id: string) {
    const r = await api.approveTravel(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const reason = prompt('Reason?') ?? ''
    const r = await api.rejectTravel(id, reason || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle title="Travel" subtitle="Requests and approvals" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">New request</h3>
        {loading ? (
          <Spinner />
        ) : (
          <form onSubmit={createReq} className="grid gap-4 sm:grid-cols-2">
            <Input label="Destination" value={destination} onChange={(e) => setDestination(e.target.value)} required className="sm:col-span-2" />
            <div className="sm:col-span-2">
              <TextArea label="Purpose" value={purpose} onChange={(e) => setPurpose(e.target.value)} required rows={2} />
            </div>
            <Input type="date" label="From" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
            <Input type="date" label="To" value={toDate} onChange={(e) => setToDate(e.target.value)} />
            <Input label="Budget (optional)" type="number" step="0.01" value={budget} onChange={(e) => setBudget(e.target.value)} />
            <div className="flex items-end">
              <Btn type="submit">Submit</Btn>
            </div>
          </form>
        )}
      </Card>

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">My travel</h3>
        <pre className="max-h-64 overflow-auto text-xs">{JSON.stringify(mine, null, 2)}</pre>
      </Card>

      {manager && (
        <Card>
          <h3 className="mb-4 text-sm font-semibold">Pending approvals</h3>
          <div className="space-y-3">
            {pending.map((row) => (
              <div key={row.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border bg-slate-50 px-4 py-2">
                <code className="text-xs">{row.id}</code>
                <div className="flex gap-2">
                  <Btn onClick={() => void approve(row.id)}>Approve</Btn>
                  <Btn variant="danger" onClick={() => void reject(row.id)}>
                    Reject
                  </Btn>
                </div>
              </div>
            ))}
            {!pending.length && <p className="text-sm text-slate-500">None pending.</p>}
          </div>
        </Card>
      )}
    </div>
  )
}
