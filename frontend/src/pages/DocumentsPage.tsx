import { useCallback, useEffect, useState } from 'react'
import * as api from '../lib/api'
import type { DocumentDto } from '../lib/types'
import { Btn, Card, Input, PageTitle, Select, TextArea, Alert, Spinner } from '../components/Ui'
import { apiErrorMessage, formatDateTime } from '../lib/util'

export default function DocumentsPage() {
  const [company, setCompany] = useState<DocumentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [documentType, setDocumentType] = useState('2')
  const [category, setCategory] = useState('General')
  const [file, setFile] = useState<File | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    const r = await api.getCompanyDocuments()
    if (r.success && r.data) setCompany(r.data)
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

  async function downloadFile(doc: DocumentDto) {
    const { access } = api.getStoredTokens()
    const url = api.downloadDocumentUrl(doc.id)
    const res = await fetch(url, { headers: access ? { Authorization: `Bearer ${access}` } : {} })
    if (!res.ok) {
      setMsg({ type: 'err', text: 'Download failed.' })
      return
    }
    const blob = await res.blob()
    const a = document.createElement('a')
    a.href = URL.createObjectURL(blob)
    a.download = doc.fileName || 'download'
    a.click()
    URL.revokeObjectURL(a.href)
  }

  return (
    <div>
      <PageTitle title="Documents" subtitle="Company library and uploads" />

      {msg && (
        <div className="mb-4">
          <Alert type={msg.type === 'ok' ? 'ok' : 'err'}>{msg.text}</Alert>
        </div>
      )}

      <Card className="mb-8">
        <h3 className="mb-4 text-sm font-semibold text-slate-800">Upload company document</h3>
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
        <h3 className="mb-4 text-sm font-semibold text-slate-800">Company documents</h3>
        {loading ? (
          <Spinner />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-slate-500">
                  <th className="pb-3 font-medium">Title</th>
                  <th className="pb-3 font-medium">Category</th>
                  <th className="pb-3 font-medium">File</th>
                  <th className="pb-3 font-medium">Uploaded by</th>
                  <th className="pb-3 font-medium">When</th>
                  <th className="pb-3 font-medium"> </th>
                </tr>
              </thead>
              <tbody>
                {company.map((doc) => (
                  <tr key={doc.id} className="border-b border-slate-100">
                    <td className="py-3 font-medium text-slate-900">{doc.title}</td>
                    <td className="py-3">{doc.category}</td>
                    <td className="max-w-[140px] truncate py-3 text-slate-600" title={doc.fileName}>
                      {doc.fileName}
                    </td>
                    <td className="py-3">{doc.uploadedByName}</td>
                    <td className="py-3 text-slate-600">{formatDateTime(doc.createdAt)}</td>
                    <td className="py-3">
                      <button
                        type="button"
                        className="text-sm font-medium text-indigo-600 hover:text-indigo-800"
                        onClick={() => void downloadFile(doc)}
                      >
                        Download
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {company.length === 0 && <p className="mt-4 text-sm text-slate-500">No documents yet.</p>}
          </div>
        )}
      </Card>
    </div>
  )
}
