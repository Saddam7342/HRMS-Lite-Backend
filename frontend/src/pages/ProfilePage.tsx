import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Alert } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'

export default function ProfilePage() {
  const { user, refreshUser } = useAuth()
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  async function changePw(e: React.FormEvent) {
    e.preventDefault()
    setMsg(null)
    const r = await api.changePassword(current, next)
    if (r.success) {
      setMsg({ type: 'ok', text: 'Password updated.' })
      setCurrent('')
      setNext('')
      await refreshUser()
    } else setMsg({ type: 'err', text: apiErrorMessage(r) })
  }

  return (
    <div>
      <PageTitle title="Profile" subtitle="Account security" />

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold text-slate-800">Session</h3>
        <dl className="grid gap-2 text-sm sm:grid-cols-2">
          <dt className="text-slate-500">Email</dt>
          <dd className="font-medium text-slate-900">{user?.email}</dd>
          <dt className="text-slate-500">Username</dt>
          <dd className="font-medium text-slate-900">{user?.username}</dd>
          <dt className="text-slate-500">Name</dt>
          <dd className="font-medium text-slate-900">
            {user?.firstName} {user?.lastName}
          </dd>
          <dt className="text-slate-500">Roles</dt>
          <dd className="text-slate-800">{(user?.roles ?? []).join(', ') || '—'}</dd>
        </dl>
      </Card>

      <Card>
        <h3 className="mb-4 text-sm font-semibold">Change password</h3>
        {msg && (
          <div className="mb-4">
            <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
          </div>
        )}
        <form onSubmit={changePw} className="max-w-md space-y-4">
          <Input
            label="Current password"
            type="password"
            value={current}
            onChange={(e) => setCurrent(e.target.value)}
            autoComplete="current-password"
            required
          />
          <Input
            label="New password"
            type="password"
            value={next}
            onChange={(e) => setNext(e.target.value)}
            autoComplete="new-password"
            required
          />
          <Btn type="submit">Update</Btn>
        </form>
      </Card>
    </div>
  )
}
