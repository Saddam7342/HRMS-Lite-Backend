import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../lib/api'
import type { DepartmentListDto, EmployeeListDto, EmployeeProfileDto } from '../lib/types'
import { Btn, Card, DetailPageSkeleton, Input, PageTitle, Select, TextArea, Alert } from '../components/Ui'
import { apiErrorMessage, formatDate } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function EmployeeDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { roles } = useAuth()
  const admin = hasRole(roles, 'Admin')

  const [profile, setProfile] = useState<EmployeeProfileDto | null>(null)
  const [depts, setDepts] = useState<DepartmentListDto[]>([])
  const [managers, setManagers] = useState<EmployeeListDto[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    gender: 'Male',
    dateOfBirth: '',
    designation: '',
    departmentId: '',
    managerId: '',
    address: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
  })

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setMsg(null)
    const [e, d, m] = await Promise.all([
      api.getEmployee(id),
      api.getDepartments(),
      api.getEmployees({ pageNumber: 1, pageSize: 200 }),
    ])
    if (e.success && e.data) {
      const p = e.data
      setProfile(p)
      setForm({
        firstName: p.firstName,
        lastName: p.lastName,
        phoneNumber: p.phoneNumber ?? '',
        gender: p.gender || 'Male',
        dateOfBirth: p.dateOfBirth?.slice(0, 10) ?? '',
        designation: p.designation ?? '',
        departmentId: p.departmentId ?? '',
        managerId: p.managerId ?? '',
        address: p.address ?? '',
        emergencyContactName: p.emergencyContactName ?? '',
        emergencyContactPhone: p.emergencyContactPhone ?? '',
      })
    } else {
      setMsg({ type: 'err', text: apiErrorMessage(e) })
    }
    if (d.success && d.data) setDepts(d.data as DepartmentListDto[])
    if (m.success && m.data) {
      const others = (m.data.items as EmployeeListDto[]).filter((x) => x.id !== id)
      setManagers(others)
    }
    setLoading(false)
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  async function save(e: React.FormEvent) {
    e.preventDefault()
    if (!id || !admin) return
    setSaving(true)
    setMsg(null)
    const body: Record<string, unknown> = {
      id,
      firstName: form.firstName,
      lastName: form.lastName,
      phoneNumber: form.phoneNumber || null,
      gender: form.gender,
      dateOfBirth: `${form.dateOfBirth}T00:00:00Z`,
      designation: form.designation || null,
      departmentId: form.departmentId || null,
      managerId: form.managerId || null,
      address: form.address || null,
      emergencyContactName: form.emergencyContactName || null,
      emergencyContactPhone: form.emergencyContactPhone || null,
    }
    const r = await api.updateEmployee(id, body)
    setSaving(false)
    setMsg(r.success ? { type: 'ok', text: 'Employee updated.' } : { type: 'err', text: apiErrorMessage(r) })
    if (r.success) await load()
  }

  if (!id) {
    return (
      <div>
        <PageTitle title="Employee" subtitle="Not found" />
        <Btn onClick={() => navigate('/employees')}>Back</Btn>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-6">
        <Link to="/employees" className="text-sm font-medium text-indigo-600 hover:text-indigo-800">
          ← Employees
        </Link>
      </div>
      <PageTitle title={profile ? `${profile.firstName} ${profile.lastName}` : 'Employee'} subtitle={profile?.employeeCode} />

      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      {loading ? (
        <DetailPageSkeleton />
      ) : profile ? (
        <div className="grid gap-8 lg:grid-cols-2">
          <Card>
            <h3 className="mb-4 text-sm font-semibold text-slate-800">Profile</h3>
            <dl className="grid gap-2 text-sm">
              <dt className="text-slate-500">Email</dt>
              <dd className="font-medium text-slate-900">{profile.email}</dd>
              <dt className="text-slate-500">Department</dt>
              <dd>{profile.departmentName ?? '—'}</dd>
              <dt className="text-slate-500">Manager</dt>
              <dd>{profile.managerName ?? '—'}</dd>
              <dt className="text-slate-500">Status</dt>
              <dd>{profile.status}</dd>
              <dt className="text-slate-500">Hire date</dt>
              <dd>{formatDate(profile.hireDate)}</dd>
            </dl>
          </Card>

          {admin ? (
            <Card>
              <h3 className="mb-4 text-sm font-semibold text-slate-800">Edit (admin)</h3>
              <form onSubmit={save} className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <Input
                    label="First name"
                    value={form.firstName}
                    onChange={(ev) => setForm({ ...form, firstName: ev.target.value })}
                    required
                  />
                  <Input
                    label="Last name"
                    value={form.lastName}
                    onChange={(ev) => setForm({ ...form, lastName: ev.target.value })}
                    required
                  />
                  <Input
                    label="Phone"
                    value={form.phoneNumber}
                    onChange={(ev) => setForm({ ...form, phoneNumber: ev.target.value })}
                  />
                  <Select label="Gender" value={form.gender} onChange={(ev) => setForm({ ...form, gender: ev.target.value })}>
                    <option>Male</option>
                    <option>Female</option>
                    <option>Other</option>
                  </Select>
                  <Input
                    type="date"
                    label="Date of birth"
                    value={form.dateOfBirth}
                    onChange={(ev) => setForm({ ...form, dateOfBirth: ev.target.value })}
                    required
                  />
                  <Input
                    label="Designation"
                    value={form.designation}
                    onChange={(ev) => setForm({ ...form, designation: ev.target.value })}
                  />
                  <Select
                    label="Department"
                    value={form.departmentId}
                    onChange={(ev) => setForm({ ...form, departmentId: ev.target.value })}
                  >
                    <option value="">—</option>
                    {depts.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.name}
                      </option>
                    ))}
                  </Select>
                  <Select
                    label="Manager"
                    value={form.managerId}
                    onChange={(ev) => setForm({ ...form, managerId: ev.target.value })}
                  >
                    <option value="">—</option>
                    {managers.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.fullName} ({x.employeeCode})
                      </option>
                    ))}
                  </Select>
                </div>
                <TextArea label="Address" value={form.address} onChange={(ev) => setForm({ ...form, address: ev.target.value })} rows={2} />
                <div className="grid gap-4 sm:grid-cols-2">
                  <Input
                    label="Emergency contact"
                    value={form.emergencyContactName}
                    onChange={(ev) => setForm({ ...form, emergencyContactName: ev.target.value })}
                  />
                  <Input
                    label="Emergency phone"
                    value={form.emergencyContactPhone}
                    onChange={(ev) => setForm({ ...form, emergencyContactPhone: ev.target.value })}
                  />
                </div>
                <Btn type="submit" disabled={saving}>
                  {saving ? 'Saving…' : 'Save changes'}
                </Btn>
              </form>
            </Card>
          ) : (
            <Card>
              <p className="text-sm text-slate-600">Only administrators can edit employee records from the web console.</p>
            </Card>
          )}
        </div>
      ) : null}
    </div>
  )
}
