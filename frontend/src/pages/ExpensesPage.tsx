import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Select, TextArea, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, todayISODate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

type Cat = { id: string; name: string }

export default function ExpensesPage() {
  const { roles } = useAuth()
  const manager = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')
  const [cats, setCats] = useState<Cat[]>([])
  const [mine, setMine] = useState<unknown[]>([])
  const [pending, setPending] = useState<Array<{ id: string }>>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [categoryId, setCategoryId] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [amount, setAmount] = useState('0')
  const [expenseDate, setExpenseDate] = useState(todayISODate())

  const load = useCallback(async () => {
    setLoading(true)
    const [c, m, p] = await Promise.all([
      api.getExpenseCategories(),
      api.getMyExpenseClaims(),
      manager ? api.getPendingExpenseClaims() : Promise.resolve({ success: true, data: [] } as const),
    ])
    if (c.success && c.data) {
      const raw = c.data as Record<string, unknown>[]
      setCats(
        raw.map((x) => ({
          id: String(x.id ?? x.Id),
          name: String(x.name ?? x.Name ?? ''),
        })),
      )
    }
    if (m.success && m.data) setMine(m.data as unknown[])
    if (p.success && p.data) setPending(p.data as { id: string }[])
    setLoading(false)
  }, [manager])

  useEffect(() => {
    void load()
  }, [load])

  async function createClaim(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const r = await api.createExpenseClaim({
      categoryId,
      title,
      description: description || null,
      amount: Number(amount),
      expenseDate: `${expenseDate}T12:00:00Z`,
    })
    setMsg(r.success ? { type: 'ok', text: 'Claim created.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function approve(id: string) {
    const r = await api.approveExpense(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const reason = prompt('Reason?') ?? ''
    const r = await api.rejectExpense(id, reason || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle title="Expense claims" subtitle="Submit and review reimbursements" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">New claim</h3>
        {loading ? (
          <Spinner />
        ) : (
          <form onSubmit={createClaim} className="grid gap-4 sm:grid-cols-2">
            <Select label="Category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
              <option value="">Select…</option>
              {cats.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </Select>
            <Input label="Title" value={title} onChange={(e) => setTitle(e.target.value)} required />
            <Input
              label="Amount"
              type="number"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              required
            />
            <Input type="date" label="Expense date" value={expenseDate} onChange={(e) => setExpenseDate(e.target.value)} />
            <div className="sm:col-span-2">
              <TextArea label="Description" value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
            </div>
            <Btn type="submit">Submit</Btn>
          </form>
        )}
      </Card>

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">My claims</h3>
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
