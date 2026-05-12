import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { DepartmentHierarchyDto, DepartmentListDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, TextArea, Alert, Spinner } from '../components/Ui'
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

  function renderTree(nodes: DepartmentHierarchyDto[], depth = 0) {
    return (
      <ul className={depth ? 'ml-4 border-l border-slate-200 pl-4' : ''}>
        {nodes.map((n) => (
          <li key={n.id} className="py-1.5">
            <span className="font-medium text-slate-800">{n.name}</span>
            <span className="ml-2 text-xs text-slate-500">{n.code}</span>
            {n.children?.length ? <div className="mt-1">{renderTree(n.children, depth + 1)}</div> : null}
          </li>
        ))}
      </ul>
    )
  }

  return (
    <div>
      <PageTitle title="Departments" subtitle="Org structure and directory" />
      {msg && <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>}

      <div className="mb-6 flex gap-2">
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
          <Spinner />
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
                  {admin && <th className="pb-3 pl-4 font-medium"> </th>}
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
                        <Btn variant="ghost" className="text-rose-600 hover:bg-rose-50" onClick={() => remove(d.id)}>
                          Delete
                        </Btn>
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
    </div>
  )
}
