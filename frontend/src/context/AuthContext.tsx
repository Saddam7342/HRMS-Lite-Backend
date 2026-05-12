import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { CurrentUserDto, LoginResponse } from '../lib/types'
import * as api from '../lib/api'

type AuthState = {
  user: CurrentUserDto | null
  roles: string[]
  permissions: string[]
  loginHint: LoginResponse | null
  bootstrapping: boolean
  login: (u: string, p: string) => Promise<{ ok: boolean; message?: string }>
  logout: () => Promise<void>
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUserDto | null>(null)
  const [roles, setRoles] = useState<string[]>([])
  const [permissions, setPermissions] = useState<string[]>([])
  const [loginHint, setLoginHint] = useState<LoginResponse | null>(null)
  const [bootstrapping, setBootstrapping] = useState(true)

  const refreshUser = useCallback(async () => {
    const { access } = api.getStoredTokens()
    if (!access) {
      setUser(null)
      setRoles([])
      setPermissions([])
      return
    }
    const r = await api.getMe()
    if (r.success && r.data) {
      setUser(r.data)
      setRoles(r.data.roles ?? [])
      setPermissions(r.data.permissions ?? [])
    } else {
      api.clearTokens()
      setUser(null)
      setRoles([])
      setPermissions([])
    }
  }, [])

  useEffect(() => {
    ;(async () => {
      await refreshUser()
      setBootstrapping(false)
    })()
  }, [refreshUser])

  const login = useCallback(async (emailOrUsername: string, password: string) => {
    const r = await api.login(emailOrUsername, password)
    if (!r.success || !r.data) {
      return { ok: false, message: r.errors?.join('; ') ?? r.message ?? 'Login failed' }
    }
    const t = r.data.token
    api.setTokens(t.accessToken, t.refreshToken)
    setLoginHint(r.data)
    setRoles(r.data.roles ?? [])
    setPermissions(r.data.permissions ?? [])
    const me = await api.getMe()
    if (me.success && me.data) setUser(me.data)
    return { ok: true }
  }, [])

  const logout = useCallback(async () => {
    const rt = api.getStoredTokens().refresh
    if (rt) {
      await api.logout(rt)
    }
    api.clearTokens()
    setUser(null)
    setRoles([])
    setPermissions([])
    setLoginHint(null)
  }, [])

  const value = useMemo(
    () => ({ user, roles, permissions, loginHint, bootstrapping, login, logout, refreshUser }),
    [user, roles, permissions, loginHint, bootstrapping, login, logout, refreshUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth outside AuthProvider')
  return ctx
}

export function hasRole(roles: string[], role: string) {
  return roles.some((r) => r.toLowerCase() === role.toLowerCase())
}
