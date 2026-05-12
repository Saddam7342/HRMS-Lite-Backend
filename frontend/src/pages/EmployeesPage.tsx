import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { DepartmentListDto, EmployeeListDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, Select, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function EmployeesPage() {
  const { roles } = useAuth()
  const canManage = hasRole(roles, 'Admin')

  const [page, setPage] = useState(1)
  const [items, setItems] = useState<EmployeeListDto[]>([])
  const [totalPages, setTotalPages] = useState(1)
  const [depts, setDepts] = useState<DepartmentListDto[]>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    employeeCode: '',
    gender: 'Male',
    dateOfBirth: '1990-01-01',
    hireDate: new Date().toISOString().slice(0, 10),
    departmentId: '',
  })

  const load = useCallback(async () => {
    setLoading(true)
    const [p, d] = await Promise.all([
      api.getEmployees({ pageNumber: page, pageSize: 10 }),
      api.getDepartments(),
    ])
    if (p.success && p.data) {
      setItems(p.data.items as EmployeeListDto[])
      setTotalPages(p.data.totalPages)
    }
    if (d.success && d.data) setDepts(d.data as DepartmentListDto[])
    setLoading(false)
  }, [page])

  useEffect(() => {
    load()
  }, [load])

  async function createEmployee(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const body: Record<string, unknown> = {
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      employeeCode: form.employeeCode,
      gender: form.gender,
      dateOfBirth: `${form.dateOfBirth}T00:00:00Z`,
      hireDate: `${form.hireDate}T00:00:00Z`,
    }
    if (form.departmentId) body.departmentId = form.departmentId
    const r = await api.createEmployee(body)
    if (r.success) {
      setMsg({ type: 'ok', text: 'Employee onboarded.' })
      await load()
    } else setMsg({ type: 'err', text: apiErrorMessage(r) })
  }

  async function toggleActive(id: string, active: boolean) {
    const r = active ? await api.deactivateEmployee(id) : await api.activateEmployee(id)
    setMsg(r.success ? { type: 'ok', text: 'Updated.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  return (
    <div>
      <PageTitle title="Employees" subtitle="Directory & onboarding" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {canManage && (
        <Card className="mb-8">
          <h3 className="mb-4 text-sm font-semibold">Onboard employee</h3>
          <form onSubmit={createEmployee} className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <Input label="First name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
            <Input label="Last name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
            <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            <Input label="Employee code" value={form.employeeCode} onChange={(e) => setForm({ ...form, employeeCode: e.target.value })} required />
            <Select label="Gender" value={form.gender} onChange={(e) => setForm({ ...form, gender: e.target.value })}>
              <option>Male</option>
              <option>Female</option>
              <option>Other</option>
            </Select>
            <Input label="Date of birth" type="date" value={form.dateOfBirth} onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} required />
            <Input label="Hire date" type="date" value={form.hireDate} onChange={(e) => setForm({ ...form, hireDate: e.target.value })} required />
            <Select
              label="Department (optional)"
              value={form.departmentId}
              onChange={(e) => setForm({ ...form, departmentId: e.target.value })}
            >
              <option value="">—</option>
              {depts.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </Select>
            <div className="flex items-end">
              <Btn type="submit">Create</Btn>
            </div>
          </form>
        </Card>
      )}

      <Card>
        {loading ? (
          <Spinner />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-200 text-slate-500">
                    <th className="pb-3 font-medium">Name</th>
                    <th className="pb-3 font-medium">Code</th>
                    <th className="pb-3 font-medium">Department</th>
                    <th className="pb-3 font-medium">Status</th>
                    {canManage && <th className="pb-3 font-medium"> </th>}
                  </tr>
                </thead>
                <tbody>
                  {items.map((e) => (
                    <tr key={e.id} className="border-b border-slate-100">
                      <td className="py-3 font-medium text-slate-900">{e.fullName}</td>
                      <td className="py-3 text-slate-600">{e.employeeCode}</td>
                      <td className="py-3">{e.departmentName ?? '—'}</td>
                      <td className="py-3">{e.status}</td>
                      {canManage && (
                        <td className="py-3">
                          <Btn
                            variant="secondary"
                            onClick={() => {
                              const active = String(e.status) === 'Active'
                              void toggleActive(e.id, active)
                            }}
                          >
                            {String(e.status) === 'Active' ? 'Deactivate' : 'Activate'}
                          </Btn>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="mt-4 flex items-center justify-between gap-4">
              <Btn variant="secondary" disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))}>
                Previous
              </Btn>
              <span className="text-sm text-slate-500">
                Page {page} of {totalPages}
              </span>
              <Btn variant="secondary" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
                Next
              </Btn>
            </div>
          </>
        )}
      </Card>
    </div>
  )
}
