import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { Btn, Card, Input, Alert, Spinner } from '../components/Ui'

export default function LoginPage() {
  const { login, user, bootstrapping } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string })?.from ?? '/'

  const [email, setEmail] = useState('admin@company.com')
  const [password, setPassword] = useState('')
  const [err, setErr] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  if (!bootstrapping && user) {
    return <Navigate to="/" replace />
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    setLoading(true)
    const r = await login(email, password)
    setLoading(false)
    if (r.ok) navigate(from, { replace: true })
    else setErr(r.message ?? 'Login failed')
  }

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden bg-slate-950 px-4">
      <div
        className="pointer-events-none absolute inset-0 opacity-80"
        style={{
          background:
            'radial-gradient(ellipse 80% 50% at 50% -20%, rgba(99,102,241,0.35), transparent), radial-gradient(ellipse 60% 40% at 100% 0%, rgba(56,189,248,0.15), transparent)',
        }}
      />
      <Card className="relative z-10 w-full max-w-md border-slate-800/60 bg-slate-900/90 p-8 shadow-2xl shadow-indigo-950/50 backdrop-blur">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-500 text-lg font-bold text-white shadow-lg shadow-indigo-500/40">
            H
          </div>
          <h1 className="text-xl font-semibold text-white">Sign in to HRMS</h1>
          <p className="mt-2 text-sm text-slate-400">Company HR & attendance in one place.</p>
        </div>
        <form onSubmit={submit} className="space-y-5">
          {err && <Alert type="err">{err}</Alert>}
          <Input
            label="Email or username"
            labelClassName="text-slate-300"
            type="text"
            autoComplete="username"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className="border-slate-600 bg-slate-800 text-white placeholder:text-slate-400 focus:border-indigo-400 focus:ring-indigo-500/40"
          />
          <Input
            label="Password"
            labelClassName="text-slate-300"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="border-slate-600 bg-slate-800 text-white placeholder:text-slate-400 focus:border-indigo-400 focus:ring-indigo-500/40"
          />
          <Btn type="submit" className="w-full py-3" disabled={loading}>
            {loading ? (
              <span className="flex items-center justify-center gap-2">
                <Spinner className="!h-4 !w-4 border-white/30 border-t-white" />
                Signing in…
              </span>
            ) : (
              'Continue'
            )}
          </Btn>
        </form>
      </Card>
    </div>
  )
}
