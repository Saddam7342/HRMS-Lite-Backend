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

export interface DepartmentDto extends DepartmentListDto {
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
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
