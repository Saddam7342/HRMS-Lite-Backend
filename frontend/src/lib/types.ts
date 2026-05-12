export interface ApiResponse<T> {
  success: boolean
  message: string | null
  data: T | null
  errors: string[] | null
  traceId: string
  timestamp: string
}

export interface TokenDto {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export interface LoginResponse {
  userId: string
  email: string
  fullName: string
  token: TokenDto
  roles: string[]
  permissions: string[]
}

export interface CurrentUserDto {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  roles: string[]
  permissions: string[]
}

export interface DepartmentListDto {
  id: string
  name: string
  code: string
  parentDepartmentName: string | null
  departmentHeadName: string | null
  isActive: boolean
  employeeCount: number
}

/** Full department row — not the same shape as list (see API DepartmentDto). */
export interface DepartmentDto {
  id: string
  name: string
  code: string
  description: string | null
  parentDepartmentId: string | null
  parentDepartmentName: string | null
  departmentHeadId: string | null
  departmentHeadName: string | null
  isActive: boolean
}

export interface DepartmentHierarchyDto {
  id: string
  name: string
  code: string
  departmentHeadName: string | null
  children: DepartmentHierarchyDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface EmployeeListDto {
  id: string
  employeeCode: string
  fullName: string
  designation: string | null
  departmentName: string | null
  status: string
  profileImageUrl: string | null
}

export interface EmployeeProfileDto {
  id: string
  employeeCode: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  gender: string
  dateOfBirth: string
  hireDate: string
  designation: string | null
  departmentId: string | null
  departmentName: string | null
  managerId: string | null
  managerName: string | null
  status: string
  address: string | null
  emergencyContactName: string | null
  emergencyContactPhone: string | null
  profileImageUrl: string | null
}

export interface LeaveRequestDto {
  id: string
  employeeId: string
  employeeName: string
  leaveTypeId: string
  leaveTypeName: string
  startDate: string
  endDate: string
  totalDays: number
  reason: string | null
  status: string
  approverName: string | null
  approvedAt: string | null
  rejectionReason: string | null
}

export interface LeaveBalanceDto {
  leaveTypeId: string
  leaveTypeName: string
  totalDays: number
  usedDays: number
  remainingDays: number
  year: number
}

export interface LeaveCalendarDto {
  id: string
  employeeId: string
  employeeName: string
  leaveTypeName: string
  startDate: string
  endDate: string
  status: string
}

export interface ExpenseClaimDto {
  id: string
  employeeId: string
  employeeName: string
  categoryId: string
  categoryName: string
  title: string
  description: string | null
  amount: number
  expenseDate: string
  status: string
  receiptFileUrl: string | null
  submittedAt: string | null
  approverName: string | null
  approvedAt: string | null
  rejectionReason: string | null
}

export interface ExpenseClaimListDto {
  id: string
  employeeName: string
  categoryName: string
  title: string
  amount: number
  expenseDate: string
  status: string
}

export interface TravelRequestDto {
  id: string
  employeeId: string
  employeeName: string
  destination: string
  purpose: string
  fromDate: string
  toDate: string
  status: string
  estimatedBudget: number | null
  approverName: string | null
  approvedAt: string | null
  rejectionReason: string | null
}

export interface TeamTravelScheduleDto {
  id: string
  employeeId: string
  employeeName: string
  destination: string
  fromDate: string
  toDate: string
  status: string
}

export interface AttendanceDto {
  id: string
  employeeId: string
  employeeName: string
  date: string
  checkInTime: string | null
  checkOutTime: string | null
  totalHours: number | null
  status: string
  isLate: boolean
  notes: string | null
}

export interface AttendanceListDto {
  id: string
  employeeName: string
  date: string
  checkInTime: string | null
  checkOutTime: string | null
  status: string
}

export interface DocumentDto {
  id: string
  title: string
  description: string | null
  fileName: string
  fileType: string
  fileSize: number
  documentType: string
  category: string
  employeeId: string | null
  employeeName: string | null
  uploadedById: string
  uploadedByName: string
  version: number
  createdAt: string
}

export interface HrDashboardDto {
  employeeSummary: {
    totalEmployees: number
    activeEmployees: number
    newHiresThisMonth: number
    departmentDistribution: { departmentName: string; count: number }[]
  }
  leaveSummary: {
    totalRequests: number
    pendingCount: number
    approvedCount: number
    rejectedCount: number
    typeDistribution: { leaveTypeName: string; count: number }[]
  }
  expenseSummary: {
    totalClaimed: number
    approvedAmount: number
    pendingAmount: number
    categorySpending: { categoryName: string; amount: number }[]
  }
  travelSummary: {
    totalRequests: number
    approvedCount: number
    pendingCount: number
    destinationDistribution: { destination: string; count: number }[]
  }
  attendanceSummary: {
    averageWorkingHours: number
    presenceRatio: number
    lateArrivalsCount: number
    missingCheckoutsCount: number
  }
}
