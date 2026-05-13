import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, PageTitle, Alert, ListSkeleton } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'

type Notif = { id: string; title: string; message: string; isRead: boolean; createdAt: string }

export default function NotificationsPage() {
  const [items, setItems] = useState<Notif[]>([])
  const [prefs, setPrefs] = useState<Record<string, boolean> | null>(null)
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    const [n, p] = await Promise.all([api.getMyNotifications(1, 50), api.getNotificationPreferences()])
    if (n.success && n.data) {
      const arr = Array.isArray(n.data) ? (n.data as Notif[]) : []
      setItems(arr)
    }
    if (p.success && p.data) setPrefs(p.data as Record<string, boolean>)
    setLoading(false)
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function markRead(id: string) {
    const r = await api.markNotificationRead(id)
    setMsg(r.success ? { type: 'ok', text: 'Marked read.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function markAll() {
    const r = await api.markAllNotificationsRead()
    setMsg(r.success ? { type: 'ok', text: 'All read.' } : { type: 'err', text: apiErrorMessage(r) })
    await load()
  }

  async function savePrefs(e: React.FormEvent) {
    e.preventDefault()
    if (!prefs) return
    const r = await api.updateNotificationPreferences(prefs)
    setMsg(r.success ? { type: 'ok', text: 'Saved preferences.' } : { type: 'err', text: apiErrorMessage(r) })
  }

  function togglePref(key: string) {
    setPrefs((p) => (p ? { ...p, [key]: !p[key] } : p))
  }

  return (
    <div>
      <PageTitle title="Notifications" subtitle="In-app messages and preferences" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <div className="mb-4 flex gap-2">
        <Btn variant="secondary" onClick={() => void markAll()}>
          Mark all read
        </Btn>
      </div>

      <Card className="mb-8">
        {loading ? (
          <ListSkeleton rows={6} />
        ) : (
          <ul className="space-y-3">
            {items.map((n) => (
              <li
                key={n.id}
                className={`rounded-xl border px-4 py-3 ${n.isRead ? 'border-slate-100 bg-white' : 'border-indigo-100 bg-indigo-50/40'}`}
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <div className="font-medium text-slate-900">{n.title}</div>
                    <div className="mt-1 text-sm text-slate-600">{n.message}</div>
                    <div className="mt-2 text-xs text-slate-400">{n.createdAt}</div>
                  </div>
                  {!n.isRead && (
                    <Btn variant="secondary" onClick={() => void markRead(n.id)}>
                      Mark read
                    </Btn>
                  )}
                </div>
              </li>
            ))}
            {items.length === 0 && <p className="text-sm text-slate-500">No notifications.</p>}
          </ul>
        )}
      </Card>

      {prefs && (
        <Card>
          <h3 className="mb-4 text-sm font-semibold">Preferences</h3>
          <form onSubmit={savePrefs} className="space-y-3">
            {Object.entries(prefs).map(([k, v]) => (
              <label key={k} className="flex items-center gap-3 text-sm">
                <input type="checkbox" checked={v} onChange={() => togglePref(k)} />
                {k}
              </label>
            ))}
            <Btn type="submit">Save</Btn>
          </form>
        </Card>
      )}
    </div>
  )
}
