import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  Activity,
  Bell,
  Building2,
  ClipboardList,
  FileText,
  LayoutDashboard,
  LogOut,
  Plane,
  Receipt,
  UserCircle2,
  Users,
} from 'lucide-react'
import { useAuth, hasRole } from '../context/AuthContext'
import { Btn } from './Ui'

const navMain = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, roles: null },
  { to: '/departments', label: 'Departments', icon: Building2, roles: ['Admin', 'Manager'] },
  { to: '/employees', label: 'Employees', icon: Users, roles: ['Admin', 'Manager'] },
  { to: '/attendance', label: 'Attendance', icon: Activity, roles: null },
  { to: '/leaves', label: 'Leaves', icon: ClipboardList, roles: null },
  { to: '/expenses', label: 'Expenses', icon: Receipt, roles: null },
  { to: '/travel', label: 'Travel', icon: Plane, roles: null },
  { to: '/documents', label: 'Documents', icon: FileText, roles: null },
  { to: '/notifications', label: 'Notifications', icon: Bell, roles: null },
]

function canSee(roles: string[] | null, userRoles: string[]) {
  if (!roles) return true
  return roles.some((r) => hasRole(userRoles, r))
}

export function AppShell() {
  const { user, roles, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex min-h-screen">
      <aside className="fixed inset-y-0 left-0 w-64 border-r border-slate-200/80 bg-[#0f172a] text-slate-300">
        <div className="flex h-16 items-center gap-2 border-b border-white/10 px-5">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-indigo-500 text-sm font-bold text-white">H</div>
          <div>
            <div className="text-sm font-semibold text-white">HRMS</div>
            <div className="text-[11px] text-slate-500">Admin console</div>
          </div>
        </div>
        <nav className="space-y-0.5 p-3">
          {navMain
            .filter((n) => canSee(n.roles, roles))
            .map((n) => (
              <NavLink
                key={n.to}
                to={n.to}
                end={n.to === '/'}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${
                    isActive ? 'bg-white/10 text-white' : 'text-slate-400 hover:bg-white/5 hover:text-white'
                  }`
                }
              >
                <n.icon className="h-4 w-4 shrink-0 opacity-80" />
                {n.label}
              </NavLink>
            ))}
          <NavLink
            to="/profile"
            className={({ isActive }) =>
              `mt-4 flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${
                isActive ? 'bg-white/10 text-white' : 'text-slate-400 hover:bg-white/5 hover:text-white'
              }`
            }
          >
            <UserCircle2 className="h-4 w-4 shrink-0 opacity-80" />
            Profile
          </NavLink>
        </nav>
        <div className="absolute bottom-0 left-0 right-0 border-t border-white/10 p-4">
          <div className="truncate text-xs text-slate-500">{user?.email}</div>
        </div>
      </aside>
      <div className="ml-64 flex min-h-screen flex-1 flex-col">
        <header className="sticky top-0 z-10 flex h-16 items-center justify-end border-b border-slate-200/80 bg-white/80 px-8 backdrop-blur">
          <Btn variant="ghost" onClick={handleLogout} className="gap-2">
            <LogOut className="h-4 w-4" />
            Sign out
          </Btn>
        </header>
        <main className="flex-1 p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
