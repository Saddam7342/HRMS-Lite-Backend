import { useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'
import { hasRole, useAuth } from '../context/AuthContext'

export default function AuditPage() {
  const { roles } = useAuth()
  const admin = hasRole(roles, 'Admin')

  const [entityName, setEntityName] = useState('Employee')
  const [entityId, setEntityId] = useState('')
  const [entityLogs, setEntityLogs] = useState<unknown>(null)

  const [userId, setUserId] = useState('')
  const [userLogs, setUserLogs] = useState<unknown>(null)

  const [systemLogs, setSystemLogs] = useState<unknown>(null)
  const [loading, setLoading] = useState(false)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  async function loadEntity() {
    setLoading(true)
    setMsg(null)
    const r = await api.getEntityAuditHistory(entityName, entityId)
    setEntityLogs(r.success ? r.data : null)
    if (!r.success) setMsg({ type: 'err', text: apiErrorMessage(r) })
    setLoading(false)
  }

  async function loadUser() {
    setLoading(true)
    const r = await api.getUserAuditActivity(userId, 50)
    setUserLogs(r.success ? r.data : null)
    if (!r.success) setMsg({ type: 'err', text: apiErrorMessage(r) })
    setLoading(false)
  }

  async function loadSystem() {
    if (!admin) {
      setMsg({ type: 'err', text: 'Admin only.' })
      return
    }
    setLoading(true)
    const r = await api.getSystemAuditLogs(1, 50)
    setSystemLogs(r.success ? r.data : null)
    if (!r.success) setMsg({ type: 'err', text: apiErrorMessage(r) })
    setLoading(false)
  }

  return (
    <div>
      <PageTitle title="Audit" subtitle="Compliance and traceability" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <div className="grid gap-8 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 text-sm font-semibold">Entity history</h3>
          <div className="space-y-4">
            <Input label="Entity name" value={entityName} onChange={(e) => setEntityName(e.target.value)} />
            <Input label="Entity id" value={entityId} onChange={(e) => setEntityId(e.target.value)} />
            <Btn onClick={() => void loadEntity()}>Load</Btn>
            {loading ? (
              <Spinner />
            ) : (
              <pre className="max-h-56 overflow-auto text-xs">{JSON.stringify(entityLogs, null, 2)}</pre>
            )}
          </div>
        </Card>

        <Card>
          <h3 className="mb-4 text-sm font-semibold">User activity</h3>
          <div className="space-y-4">
            <Input label="User id (Guid)" value={userId} onChange={(e) => setUserId(e.target.value)} />
            <Btn onClick={() => void loadUser()}>Load</Btn>
            {loading ? (
              <Spinner />
            ) : (
              <pre className="max-h-56 overflow-auto text-xs">{JSON.stringify(userLogs, null, 2)}</pre>
            )}
          </div>
        </Card>
      </div>

      {admin && (
        <Card className="mt-8">
          <h3 className="mb-4 text-sm font-semibold">System audit log</h3>
          <Btn onClick={() => void loadSystem()}>Load latest</Btn>
          <pre className="mt-4 max-h-72 overflow-auto text-xs">{JSON.stringify(systemLogs, null, 2)}</pre>
        </Card>
      )}
    </div>
  )
}
