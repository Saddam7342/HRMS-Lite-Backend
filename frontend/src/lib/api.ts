import type {
  ApiResponse,
  AttendanceDto,
  AttendanceListDto,
  CurrentUserDto,
  DepartmentDto,
  DepartmentHierarchyDto,
  DepartmentListDto,
  EmployeeListDto,
  EmployeeProfileDto,
  LoginResponse,
  PagedResult,
  TokenDto,
  HrDashboardDto,
  ExpenseClaimDto,
  ExpenseClaimListDto,
  LeaveRequestDto,
  LeaveCalendarDto,
  TravelRequestDto,
  TeamTravelScheduleDto,
  DocumentDto,
} from './types'

const TOKEN_ACCESS = 'hrms_access_token'
const TOKEN_REFRESH = 'hrms_refresh_token'

export function apiPath(path: string): string {
  const base = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')
  const p = path.startsWith('/') ? path : `/${path}`
  if (base) return `${base}${p}`
  return p
}

export function getStoredTokens() {
  return {
    access: localStorage.getItem(TOKEN_ACCESS),
    refresh: localStorage.getItem(TOKEN_REFRESH),
  }
}

export function setTokens(access: string, refresh: string) {
  localStorage.setItem(TOKEN_ACCESS, access)
  localStorage.setItem(TOKEN_REFRESH, refresh)
}

export function clearTokens() {
  localStorage.removeItem(TOKEN_ACCESS)
  localStorage.removeItem(TOKEN_REFRESH)
}

async function tryRefresh(): Promise<boolean> {
  const { access, refresh } = getStoredTokens()
  if (!access || !refresh) return false
  try {
    const res = await fetch(apiPath('/api/v1/Auth/refresh'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ accessToken: access, refreshToken: refresh }),
    })
    if (!res.ok) return false
    const text = await res.text()
    if (!text) return false
    const j = JSON.parse(text) as ApiResponse<TokenDto>
    if (!j.success || !j.data) return false
    setTokens(j.data.accessToken, j.data.refreshToken)
    return true
  } catch {
    return false
  }
}

type Opt = RequestInit & { skipAuth?: boolean; raw?: boolean }

export async function request<T>(path: string, init?: Opt): Promise<ApiResponse<T>> {
  const headers = new Headers(init?.headers)
  if (!init?.skipAuth) {
    const at = getStoredTokens().access
    if (at) headers.set('Authorization', `Bearer ${at}`)
  }
  const body = init?.body
  const isForm = body instanceof FormData
  if (!isForm && body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const opts: RequestInit = { ...init, headers }
  let res = await fetch(apiPath(path), opts)

  if (res.status === 401 && !init?.skipAuth && (await tryRefresh())) {
    const at = getStoredTokens().access!
    headers.set('Authorization', `Bearer ${at}`)
    res = await fetch(apiPath(path), { ...init, headers })
  }

  if (init?.raw) return { success: res.ok } as unknown as ApiResponse<T>

  const text = await res.text()
  if (!text) {
    return { success: res.ok, message: res.statusText } as ApiResponse<T>
  }

  try {
    const json = JSON.parse(text)
    // Handle potential casing differences from backend
    if (json.Success !== undefined && json.success === undefined) {
      json.success = json.Success
    }
    return json as ApiResponse<T>
  } catch {
    return { success: false, message: 'Invalid server response' } as ApiResponse<T>
  }
}

// --- Auth

export function login(emailOrUsername: string, password: string) {
  return request<LoginResponse>('/api/v1/Auth/login', {
    method: 'POST',
    body: JSON.stringify({ emailOrUsername, password }),
    skipAuth: true,
  })
}

export function logout(refreshToken: string) {
  return request<unknown>('/api/v1/Auth/logout', {
    method: 'POST',
    body: JSON.stringify({ refreshToken }),
  })
}

export function getMe() {
  return request<CurrentUserDto>('/api/v1/Auth/me')
}

export function changePassword(currentPassword: string, newPassword: string) {
  return request<unknown>('/api/v1/Auth/change-password', {
    method: 'POST',
    body: JSON.stringify({ currentPassword, newPassword }),
  })
}

// --- Departments

export function getDepartments() {
  return request<DepartmentListDto[]>('/api/v1/Departments')
}

export function getDepartmentHierarchy() {
  return request<DepartmentHierarchyDto[]>('/api/v1/Departments/hierarchy')
}

export function createDepartment(body: { name: string; code: string; description?: string | null }) {
  return request<string>('/api/v1/Departments', { method: 'POST', body: JSON.stringify(body) })
}

export function getDepartment(id: string) {
  return request<DepartmentDto>(`/api/v1/Departments/${id}`)
}

export function updateDepartment(
  id: string,
  body: {
    id: string
    name: string
    code: string
    description?: string | null
    parentDepartmentId?: string | null
    departmentHeadId?: string | null
  },
) {
  return request<unknown>(`/api/v1/Departments/${id}`, { method: 'PUT', body: JSON.stringify(body) })
}

export function deleteDepartment(id: string) {
  return request<unknown>(`/api/v1/Departments/${id}`, { method: 'DELETE' })
}

export function activateDepartment(id: string) {
  return request<unknown>(`/api/v1/Departments/${id}/activate`, { method: 'PUT' })
}

export function deactivateDepartment(id: string) {
  return request<unknown>(`/api/v1/Departments/${id}/deactivate`, { method: 'PUT' })
}

export function getDepartmentEmployees(id: string) {
  return request<
    { id: string; fullName: string; designation: string | null; profileImageUrl: string | null; isHead: boolean }[]
  >(`/api/v1/Departments/${id}/employees`)
}

// --- Employees

export function getEmployees(params: {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  sortBy?: string
  sortDescending?: boolean
}) {
  const q = new URLSearchParams()
  if (params.pageNumber) q.set('pageNumber', String(params.pageNumber))
  if (params.pageSize) q.set('pageSize', String(params.pageSize))
  if (params.searchTerm) q.set('searchTerm', params.searchTerm)
  if (params.sortBy) q.set('sortBy', params.sortBy)
  if (params.sortDescending !== undefined) q.set('sortDescending', String(params.sortDescending))
  const qs = q.toString()
  return request<PagedResult<EmployeeListDto>>(`/api/v1/Employees${qs ? `?${qs}` : ''}`)
}

export function createEmployee(body: Record<string, unknown>) {
  return request<string>('/api/v1/Employees', { method: 'POST', body: JSON.stringify(body) })
}

export function getEmployee(id: string) {
  return request<EmployeeProfileDto>(`/api/v1/Employees/${id}`)
}

export function getMyEmployeeProfile() {
  return request<EmployeeProfileDto>('/api/v1/Employees/me')
}

export function updateEmployee(id: string, body: Record<string, unknown>) {
  return request<unknown>(`/api/v1/Employees/${id}`, { method: 'PUT', body: JSON.stringify({ ...body, id }) })
}

export function activateEmployee(id: string) {
  return request<unknown>(`/api/v1/Employees/${id}/activate`, { method: 'PUT' })
}

export function deactivateEmployee(id: string) {
  return request<unknown>(`/api/v1/Employees/${id}/deactivate`, { method: 'PUT' })
}

export function getMyTeam() {
  return request<unknown[]>('/api/v1/Employees/my-team')
}

export function uploadEmployeeImage(id: string, file: File) {
  const fd = new FormData()
  fd.append('file', file)
  return request<string>(`/api/v1/Employees/${id}/profile-image`, { method: 'POST', body: fd })
}

// --- Attendance (web admin — read-only listings; check-in/out reserved for mobile)

export function getAttendanceRange(start: string, end: string) {
  return request<AttendanceDto[]>(`/api/v1/Attendance/range?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`)
}

export function getTeamAttendance(date: string) {
  return request<AttendanceListDto[]>(`/api/v1/Attendance/team?date=${encodeURIComponent(date)}`)
}

export function updateAttendance(id: string, body: Record<string, unknown>) {
  return request<unknown>(`/api/v1/Attendance/${id}`, { method: 'PUT', body: JSON.stringify({ ...body, id }) })
}

export function markAbsent(id: string, date: string) {
  return request<unknown>(`/api/v1/Attendance/${id}/mark-absent?date=${encodeURIComponent(date)}`, { method: 'PUT' })
}

// --- Leaves

export function createLeave(body: { leaveTypeId: string; startDate: string; endDate: string; reason?: string | null }) {
  return request<string>('/api/v1/Leaves', { method: 'POST', body: JSON.stringify(body) })
}

export function getMyLeaves() {
  return request<unknown[]>('/api/v1/Leaves/my')
}

export function getLeaveBalances(year?: number) {
  const q = year != null ? `?year=${year}` : ''
  return request<unknown[]>(`/api/v1/Leaves/balances${q}`)
}

export function getAllLeaves() {
  return request<LeaveRequestDto[]>('/api/v1/Leaves')
}

export function getPendingLeaves() {
  return request<LeaveRequestDto[]>('/api/v1/Leaves/pending-approvals')
}

export function approveLeave(id: string) {
  return request<unknown>(`/api/v1/Leaves/${id}/approve`, { method: 'PUT' })
}

export function rejectLeave(id: string, reason?: string | null) {
  return request<unknown>(`/api/v1/Leaves/${id}/reject`, {
    method: 'PUT',
    body: JSON.stringify({ reason: reason ?? null }),
  })
}

export function cancelLeave(id: string) {
  return request<unknown>(`/api/v1/Leaves/${id}/cancel`, { method: 'PUT' })
}

export function getTeamLeaveCalendar(start: string, end: string) {
  return request<LeaveCalendarDto[]>(
    `/api/v1/Leaves/team-calendar?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`,
  )
}

// --- Expenses

export function createExpenseClaim(body: Record<string, unknown>) {
  return request<string>('/api/v1/ExpenseClaims', { method: 'POST', body: JSON.stringify(body) })
}

export function getMyExpenseClaims() {
  return request<unknown[]>('/api/v1/ExpenseClaims/my')
}

export function getExpenseClaim(id: string) {
  return request<unknown>(`/api/v1/ExpenseClaims/${id}`)
}

export function getPendingExpenseClaims() {
  return request<ExpenseClaimDto[]>('/api/v1/ExpenseClaims/pending-approvals')
}

export function getTeamExpenseClaims() {
  return request<ExpenseClaimListDto[]>('/api/v1/ExpenseClaims/team')
}

export function getAllExpenses() {
  return request<ExpenseClaimDto[]>('/api/v1/ExpenseClaims')
}

export function getExpenseCategories() {
  return request<unknown[]>('/api/v1/ExpenseClaims/categories')
}

export function approveExpense(id: string) {
  return request<unknown>(`/api/v1/ExpenseClaims/${id}/approve`, { method: 'PUT' })
}

export function rejectExpense(id: string, reason?: string | null) {
  return request<unknown>(`/api/v1/ExpenseClaims/${id}/reject`, {
    method: 'PUT',
    body: JSON.stringify({ reason: reason ?? null }),
  })
}

export function uploadExpenseReceipt(id: string, file: File) {
  const fd = new FormData()
  fd.append('file', file)
  return request<string>(`/api/v1/ExpenseClaims/${id}/receipt`, { method: 'POST', body: fd })
}

// --- Travel

export function createTravel(body: Record<string, unknown>) {
  return request<string>('/api/v1/TravelRequests', { method: 'POST', body: JSON.stringify(body) })
}

export function getMyTravel() {
  return request<unknown[]>('/api/v1/TravelRequests/my')
}

export function getTravel(id: string) {
  return request<unknown>(`/api/v1/TravelRequests/${id}`)
}

export function updateTravel(id: string, body: Record<string, unknown>) {
  return request<unknown>(`/api/v1/TravelRequests/${id}`, { method: 'PUT', body: JSON.stringify(body) })
}

export function cancelTravel(id: string) {
  return request<unknown>(`/api/v1/TravelRequests/${id}/cancel`, { method: 'PUT' })
}

export function getPendingTravel() {
  return request<TravelRequestDto[]>('/api/v1/TravelRequests/pending-approvals')
}

export function getAllTravel() {
  return request<TravelRequestDto[]>('/api/v1/TravelRequests')
}

export function getTeamTravelSchedule(start: string, end: string) {
  return request<TeamTravelScheduleDto[]>(
    `/api/v1/TravelRequests/team-schedule?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`,
  )
}

export function getTravelHistory() {
  return request<unknown[]>('/api/v1/TravelRequests/history')
}

export function approveTravel(id: string) {
  return request<unknown>(`/api/v1/TravelRequests/${id}/approve`, { method: 'PUT' })
}

export function rejectTravel(id: string, reason?: string | null) {
  return request<unknown>(`/api/v1/TravelRequests/${id}/reject`, {
    method: 'PUT',
    body: JSON.stringify({ reason: reason ?? null }),
  })
}

// --- Documents

export function uploadDocument(fd: FormData) {
  return request<string>('/api/v1/Documents/upload', { method: 'POST', body: fd })
}

export function getDocument(id: string) {
  return request<unknown>(`/api/v1/Documents/${id}`)
}

export function getEmployeeDocuments(employeeId: string) {
  return request<unknown[]>(`/api/v1/Documents/employee/${employeeId}`)
}

export function getCompanyDocuments() {
  return request<DocumentDto[]>('/api/v1/Documents/company')
}

export function updateDocument(id: string, body: { title: string; description?: string | null; category: string }) {
  return request<unknown>(`/api/v1/Documents/${id}`, { method: 'PUT', body: JSON.stringify(body) })
}

export function deleteDocument(id: string) {
  return request<unknown>(`/api/v1/Documents/${id}`, { method: 'DELETE' })
}

export function uploadDocumentVersion(id: string, file: File) {
  const fd = new FormData()
  fd.append('file', file)
  return request<unknown>(`/api/v1/Documents/${id}/version`, { method: 'POST', body: fd })
}

export function downloadDocumentUrl(id: string) {
  return apiPath(`/api/v1/Documents/${id}/download`)
}

// --- Notifications

export function getMyNotifications(page = 1, pageSize = 20) {
  return request<unknown>(`/api/v1/Notifications/my?page=${page}&pageSize=${pageSize}`)
}

export function getNotificationCount() {
  return request<unknown>('/api/v1/Notifications/count')
}

export function markNotificationRead(id: string) {
  return request<unknown>(`/api/v1/Notifications/${id}/read`, { method: 'PUT' })
}

export function markAllNotificationsRead() {
  return request<unknown>('/api/v1/Notifications/read-all', { method: 'PUT' })
}

export function deleteNotification(id: string) {
  return request<unknown>(`/api/v1/Notifications/${id}`, { method: 'DELETE' })
}

export function getNotificationPreferences() {
  return request<Record<string, boolean>>('/api/v1/Notifications/preferences')
}

export function updateNotificationPreferences(body: Record<string, boolean>) {
  return request<unknown>('/api/v1/Notifications/preferences', { method: 'PUT', body: JSON.stringify(body) })
}

// --- Reports (dashboard summary only; detailed report endpoints removed from UI)

export function getHrDashboard() {
  return request<HrDashboardDto>('/api/v1/Reports/hr-dashboard')
}

// --- System (status for admin dashboard card)

export function getSystemStatus() {
  return request<Record<string, unknown>>('/api/v1/System/status')
}
