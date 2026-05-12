import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { ExpenseClaimDto, ExpenseClaimListDto } from '../lib/types'
import { Btn, Card, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, formatDate, formatDateTime, money } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function ExpensesPage() {
  const { roles } = useAuth()
  const approver = hasRole(roles, 'Admin') || hasRole(roles, 'Manager')

  const [pending, setPending] = useState<ExpenseClaimDto[]>([])
  const [team, setTeam] = useState<ExpenseClaimListDto[]>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    const jobs: Promise<void>[] = []
    if (approver) {
      jobs.push(
        api.getPendingExpenseClaims().then((r) => {
          if (r.success && r.data) setPending(r.data)
        }),
      )
      jobs.push(
        api.getTeamExpenseClaims().then((r) => {
          if (r.success && r.data) setTeam(r.data)
        }),
      )
    } else {
      setPending([])
      setTeam([])
    }
    await Promise.all(jobs)
    setLoading(false)
  }, [approver])

  useEffect(() => {
    void load()
  }, [load])

  async function approve(id: string) {
    const r = await api.approveExpense(id)
    setMsg(r.success ? { type: 'ok', text: 'Approved.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function reject(id: string) {
    const reason = window.prompt('Reason for rejection?') ?? ''
    const r = await api.rejectExpense(id, reason || null)
    setMsg(r.success ? { type: 'ok', text: 'Rejected.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle
        title="Expense claims"
        subtitle="Review pending claims and team submissions (submission happens on mobile)."
      />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {!approver && <Alert type="info">Expense approvals are available to Admin and Manager roles.</Alert>}

      {approver && (
        <>
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
                      <th className="pb-3 font-medium">Title</th>
                      <th className="pb-3 font-medium">Category</th>
                      <th className="pb-3 font-medium">Amount</th>
                      <th className="pb-3 font-medium">Expense date</th>
                      <th className="pb-3 font-medium">Submitted</th>
                      <th className="pb-3 font-medium text-right"> </th>
                    </tr>
                  </thead>
                  <tbody>
                    {pending.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium text-slate-900">{row.employeeName}</td>
                        <td className="py-3">{row.title}</td>
                        <td className="py-3">{row.categoryName}</td>
                        <td className="py-3">{money(row.amount)}</td>
                        <td className="py-3 text-slate-600">{formatDate(row.expenseDate)}</td>
                        <td className="py-3 text-slate-600">{formatDateTime(row.submittedAt)}</td>
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
                {pending.length === 0 && <p className="mt-4 text-sm text-slate-500">No pending claims.</p>}
              </div>
            )}
          </Card>

          <Card>
            <h3 className="mb-4 text-sm font-semibold text-slate-800">Team expense claims</h3>
            {loading ? (
              <Spinner />
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-slate-500">
                      <th className="pb-3 font-medium">Employee</th>
                      <th className="pb-3 font-medium">Title</th>
                      <th className="pb-3 font-medium">Category</th>
                      <th className="pb-3 font-medium">Amount</th>
                      <th className="pb-3 font-medium">Date</th>
                      <th className="pb-3 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {team.map((row) => (
                      <tr key={row.id} className="border-b border-slate-100">
                        <td className="py-3 font-medium">{row.employeeName}</td>
                        <td className="py-3">{row.title}</td>
                        <td className="py-3">{row.categoryName}</td>
                        <td className="py-3">{money(row.amount)}</td>
                        <td className="py-3">{formatDate(row.expenseDate)}</td>
                        <td className="py-3">{row.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {team.length === 0 && <p className="mt-4 text-sm text-slate-500">No team claims found.</p>}
              </div>
            )}
          </Card>
        </>
      )}
    </div>
  )
}
