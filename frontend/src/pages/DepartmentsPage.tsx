import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { DepartmentHierarchyDto, DepartmentListDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, TextArea, Alert, Select, Skeleton, TableSkeleton, TreeSkeleton } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function DepartmentsPage() {
  const { roles } = useAuth()
  const admin = hasRole(roles, 'Admin')

  const [list, setList] = useState<DepartmentListDto[]>([])
  const [tree, setTree] = useState<DepartmentHierarchyDto[]>([])
  const [tab, setTab] = useState<'list' | 'tree'>('list')
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [name, setName] = useState('')
  const [code, setCode] = useState('')
  const [desc, setDesc] = useState('')

  const [editOpen, setEditOpen] = useState(false)
  const [editId, setEditId] = useState<string | null>(null)
  const [editForm, setEditForm] = useState({
    name: '',
    code: '',
    description: '',
    parentDepartmentId: '',
    departmentHeadId: '',
  })
  const [editLoading, setEditLoading] = useState(false)
  const [headChoices, setHeadChoices] = useState<{ id: string; fullName: string }[]>([])

  const load = useCallback(async () => {
    setLoading(true)
    const [a, b] = await Promise.all([api.getDepartments(), api.getDepartmentHierarchy()])
    if (a.success && a.data) setList(a.data as DepartmentListDto[])
    if (b.success && b.data) setTree(b.data as DepartmentHierarchyDto[])
    setLoading(false)
  }, [])

  useEffect(() => {
    load()
  }, [load])

  async function createDept(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const r = await api.createDepartment({ name, code, description: desc || null })
    if (r.success) {
      setMsg({ type: 'ok', text: 'Department created.' })
      setName('')
      setCode('')
      setDesc('')
      await load()
    } else setMsg({ type: 'err', text: apiErrorMessage(r) })
  }

  async function remove(id: string) {
    if (!confirm('Delete this department?')) return
    const r = await api.deleteDepartment(id)
    setMsg(r.success ? { type: 'ok', text: 'Deleted.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function toggleActive(d: DepartmentListDto) {
    const r = d.isActive ? await api.deactivateDepartment(d.id) : await api.activateDepartment(d.id)
    setMsg(r.success ? { type: 'ok', text: 'Updated.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function openEdit(id: string) {
    setEditId(id)
    setEditOpen(true)
    setEditLoading(true)
    setMsg(null)
    const [dep, emps] = await Promise.all([
      api.getDepartment(id),
      api.getEmployees({ pageNumber: 1, pageSize: 300 }),
    ])
    if (dep.success && dep.data) {
      const d = dep.data
      setEditForm({
        name: d.name,
        code: d.code,
        description: d.description ?? '',
        parentDepartmentId: d.parentDepartmentId ?? '',
        departmentHeadId: d.departmentHeadId ?? '',
      })
    }
    if (emps.success && emps.data)
      setHeadChoices(
        (emps.data.items as { id: string; fullName: string }[]).map((x) => ({
          id: x.id,
          fullName: x.fullName,
        })),
      )
    setEditLoading(false)
  }

  async function saveEdit(e: React.FormEvent) {
    e.preventDefault()
    if (!editId || !admin) return
    setEditLoading(true)
    const r = await api.updateDepartment(editId, {
      id: editId,
      name: editForm.name,
      code: editForm.code,
      description: editForm.description || null,
      parentDepartmentId: editForm.parentDepartmentId || null,
      departmentHeadId: editForm.departmentHeadId || null,
    })
    setEditLoading(false)
    setMsg(r.success ? { type: 'ok', text: 'Department updated.' } : { type: 'err', text: apiErrorMessage(r) })
    if (r.success) {
      setEditOpen(false)
      setEditId(null)
      await load()
    }
  }

  function renderTree(nodes: DepartmentHierarchyDto[], depth = 0) {
    return (
      <ul className={depth ? 'ml-4 border-l border-slate-200 pl-4' : ''}>
        {nodes.map((n) => (
          <li key={n.id} className="py-1.5">
            <span className="font-medium text-slate-800">{n.name}</span>
            <span className="ml-2 text-xs text-slate-500">{n.code}</span>
            {n.departmentHeadName && (
              <span className="ml-2 text-xs text-slate-400">Head: {n.departmentHeadName}</span>
            )}
            {n.children?.length ? <div className="mt-1">{renderTree(n.children, depth + 1)}</div> : null}
          </li>
        ))}
      </ul>
    )
  }

  const parentOptions = list.filter((d) => d.id !== editId)

  return (
    <div>
      <PageTitle title="Departments" subtitle="Org structure, hierarchy, and administration" />
      {msg && <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>}

      <div className="mb-6 mt-4 flex gap-2">
        <Btn variant={tab === 'list' ? 'primary' : 'secondary'} onClick={() => setTab('list')}>
          List
        </Btn>
        <Btn variant={tab === 'tree' ? 'primary' : 'secondary'} onClick={() => setTab('tree')}>
          Hierarchy
        </Btn>
      </div>

      {admin && (
        <Card className="mb-8">
          <h3 className="mb-4 text-sm font-semibold text-slate-800">New department</h3>
          <form onSubmit={createDept} className="grid gap-4 sm:grid-cols-2">
            <Input label="Name" value={name} onChange={(e) => setName(e.target.value)} required />
            <Input label="Code" value={code} onChange={(e) => setCode(e.target.value)} required />
            <div className="sm:col-span-2">
              <TextArea label="Description" value={desc} onChange={(e) => setDesc(e.target.value)} rows={2} />
            </div>
            <div className="sm:col-span-2">
              <Btn type="submit">Create</Btn>
            </div>
          </form>
        </Card>
      )}

      <Card>
        {loading ? (
          tab === 'list' ? (
            <TableSkeleton rows={6} columns={admin ? 6 : 5} />
          ) : (
            <TreeSkeleton depth={6} />
          )
        ) : tab === 'list' ? (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-slate-500">
                  <th className="pb-3 pr-4 font-medium">Name</th>
                  <th className="pb-3 pr-4 font-medium">Code</th>
                  <th className="pb-3 pr-4 font-medium">Head</th>
                  <th className="pb-3 pr-4 font-medium">Employees</th>
                  <th className="pb-3 font-medium">Active</th>
                  {admin && <th className="pb-3 pl-4 font-medium text-right"> </th>}
                </tr>
              </thead>
              <tbody>
                {list.map((d) => (
                  <tr key={d.id} className="border-b border-slate-100">
                    <td className="py-3 pr-4 font-medium text-slate-900">{d.name}</td>
                    <td className="py-3 pr-4 text-slate-600">{d.code}</td>
                    <td className="py-3 pr-4 text-slate-600">{d.departmentHeadName ?? '—'}</td>
                    <td className="py-3 pr-4">{d.employeeCount}</td>
                    <td className="py-3">{d.isActive ? 'Yes' : 'No'}</td>
                    {admin && (
                      <td className="py-3 pl-4 text-right">
                        <div className="flex flex-wrap justify-end gap-2">
                          <Btn variant="secondary" onClick={() => void openEdit(d.id)}>
                            Edit
                          </Btn>
                          <Btn variant="ghost" onClick={() => void toggleActive(d)}>
                            {d.isActive ? 'Deactivate' : 'Activate'}
                          </Btn>
                          <Btn variant="ghost" className="text-rose-600 hover:bg-rose-50" onClick={() => remove(d.id)}>
                            Delete
                          </Btn>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          renderTree(tree)
        )}
      </Card>

      {editOpen && admin && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <Card className="relative max-h-[90vh] w-full max-w-lg overflow-y-auto shadow-2xl">
            <button
              type="button"
              className="absolute right-4 top-4 text-sm text-slate-400 hover:text-slate-700"
              onClick={() => setEditOpen(false)}
            >
              ✕
            </button>
            <h3 className="mb-4 text-sm font-semibold text-slate-800">Edit department</h3>
            {editLoading ? (
              <div className="space-y-4" aria-busy>
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-20 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            ) : (
              <form onSubmit={saveEdit} className="space-y-4">
                <Input label="Name" value={editForm.name} onChange={(e) => setEditForm({ ...editForm, name: e.target.value })} required />
                <Input label="Code" value={editForm.code} onChange={(e) => setEditForm({ ...editForm, code: e.target.value })} required />
                <TextArea
                  label="Description"
                  value={editForm.description}
                  onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                  rows={2}
                />
                <Select
                  label="Parent department"
                  value={editForm.parentDepartmentId}
                  onChange={(e) => setEditForm({ ...editForm, parentDepartmentId: e.target.value })}
                >
                  <option value="">— None —</option>
                  {parentOptions.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name}
                    </option>
                  ))}
                </Select>
                <Select
                  label="Department head"
                  value={editForm.departmentHeadId}
                  onChange={(e) => setEditForm({ ...editForm, departmentHeadId: e.target.value })}
                >
                  <option value="">— None —</option>
                  {headChoices.map((h) => (
                    <option key={h.id} value={h.id}>
                      {h.fullName}
                    </option>
                  ))}
                </Select>
                <div className="flex gap-2">
                  <Btn type="submit" disabled={editLoading}>
                    Save
                  </Btn>
                  <Btn type="button" variant="secondary" onClick={() => setEditOpen(false)}>
                    Cancel
                  </Btn>
                </div>
              </form>
            )}
          </Card>
        </div>
      )}
    </div>
  )
}
