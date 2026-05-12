import { useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'

export default function SystemPage() {
  const [status, setStatus] = useState<unknown>(null)
  const [prefix, setPrefix] = useState('')
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    void (async () => {
      const r = await api.getSystemStatus()
      if (r.success) setStatus(r.data)
      setLoading(false)
    })()
  }, [])

  async function clearCache() {
    setMsg(null)
    const r = await api.clearSystemCache(prefix || null)
    setMsg(r.success ? { type: 'ok', text: 'Cache cleared.' } : { type: 'err', text: apiErrorMessage(r) })
  }

  return (
    <div>
      <PageTitle title="System" subtitle="Health & maintenance (admin)" />
      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">Status</h3>
        {loading ? <Spinner /> : <pre className="text-xs">{JSON.stringify(status, null, 2)}</pre>}
      </Card>

      <Card>
        <h3 className="mb-4 text-sm font-semibold">Clear cache</h3>
        <div className="flex flex-wrap items-end gap-4">
          <Input
            label="Prefix (optional)"
            value={prefix}
            onChange={(e) => setPrefix(e.target.value)}
            placeholder="Leave empty for full clear"
          />
          <Btn variant="danger" onClick={() => void clearCache()}>
            Clear
          </Btn>
        </div>
      </Card>
    </div>
  )
}
