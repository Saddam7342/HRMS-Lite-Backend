import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import { Btn, Card, Input, PageTitle, Select, TextArea, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage } from '../lib/util'

export default function DocumentsPage() {
  const [company, setCompany] = useState<unknown[]>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [documentType, setDocumentType] = useState('1')
  const [category, setCategory] = useState('General')
  const [file, setFile] = useState<File | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    const r = await api.getCompanyDocuments()
    if (r.success && r.data) setCompany(r.data as unknown[])
    setLoading(false)
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function upload(e: React.FormEvent) {
    e.preventDefault()
    if (!file) {
      setMsg({ type: 'err', text: 'Choose a file.' })
      return
    }
    setMsg(null)
    const fd = new FormData()
    fd.append('Title', title)
    fd.append('Description', description)
    fd.append('DocumentType', documentType)
    fd.append('Category', category)
    fd.append('File', file)
    const r = await api.uploadDocument(fd)
    setMsg(r.success ? { type: 'ok', text: 'Uploaded.' } : { type: 'err', text: apiErrorMessage(r) })
    setTitle('')
    setDescription('')
    setFile(null)
    await load()
  }

  return (
    <div>
      <PageTitle title="Documents" subtitle="Company library & uploads" />

      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold">Upload</h3>
        <form onSubmit={upload} className="grid gap-4 sm:grid-cols-2">
          <Input label="Title" value={title} onChange={(e) => setTitle(e.target.value)} required className="sm:col-span-2" />
          <div className="sm:col-span-2">
            <TextArea label="Description" value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
          </div>
          <Select label="Type" value={documentType} onChange={(e) => setDocumentType(e.target.value)}>
            <option value="1">Employee</option>
            <option value="2">Company</option>
          </Select>
          <Input label="Category" value={category} onChange={(e) => setCategory(e.target.value)} />
          <label className="sm:col-span-2">
            <span className="mb-1.5 block text-sm font-medium text-slate-600">File</span>
            <input
              type="file"
              className="block w-full text-sm text-slate-600"
              onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            />
          </label>
          <Btn type="submit">Upload</Btn>
        </form>
      </Card>

      <Card>
        <h3 className="mb-4 text-sm font-semibold">Company documents</h3>
        {loading ? (
          <Spinner />
        ) : (
          <pre className="max-h-96 overflow-auto text-xs">{JSON.stringify(company, null, 2)}</pre>
        )}
      </Card>
    </div>
  )
}
