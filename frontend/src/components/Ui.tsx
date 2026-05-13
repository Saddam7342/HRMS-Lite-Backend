import type { ReactNode } from 'react'
import { twMerge } from 'tailwind-merge'

export function Card({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <div
      className={twMerge(
        'rounded-2xl border border-slate-200/80 bg-white p-6 shadow-sm shadow-slate-200/50',
        className,
      )}
    >
      {children}
    </div>
  )
}

export function Btn({
  children,
  onClick,
  type = 'button',
  variant = 'primary',
  disabled,
  className = '',
}: {
  children: ReactNode
  onClick?: () => void
  type?: 'button' | 'submit'
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  disabled?: boolean
  className?: string
}) {
  const styles = {
    primary: 'bg-indigo-600 text-white hover:bg-indigo-700 shadow-sm shadow-indigo-600/20',
    secondary: 'bg-white text-slate-800 border border-slate-200 hover:bg-slate-50',
    ghost: 'text-slate-600 hover:bg-slate-100',
    danger: 'bg-rose-600 text-white hover:bg-rose-700',
  }
  return (
    <button
      type={type}
      disabled={disabled}
      onClick={onClick}
      className={`inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium transition disabled:opacity-50 ${styles[variant]} ${className}`}
    >
      {children}
    </button>
  )
}

export function Input(
  props: React.InputHTMLAttributes<HTMLInputElement> & { label?: string; labelClassName?: string },
) {
  const { label, labelClassName = '', className = '', id, ...rest } = props
  const tid = id ?? rest.name
  return (
    <label className="block">
      {label && (
        <span
          className={twMerge('mb-1.5 block text-sm font-medium text-slate-600', labelClassName)}
        >
          {label}
        </span>
      )}
      <input
        id={tid}
        className={twMerge(
          'w-full rounded-xl border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-900 outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/30',
          className,
        )}
        {...rest}
      />
    </label>
  )
}

export function Select(props: React.SelectHTMLAttributes<HTMLSelectElement> & { label?: string }) {
  const { label, className = '', children, ...rest } = props
  return (
    <label className="block">
      {label && <span className="mb-1.5 block text-sm font-medium text-slate-600">{label}</span>}
      <select
        className={twMerge(
          'w-full rounded-xl border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-900 outline-none focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/30',
          className,
        )}
        {...rest}
      >
        {children}
      </select>
    </label>
  )
}

export function TextArea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement> & { label?: string }) {
  const { label, className = '', ...rest } = props
  return (
    <label className="block">
      {label && <span className="mb-1.5 block text-sm font-medium text-slate-600">{label}</span>}
      <textarea
        className={twMerge(
          'w-full rounded-xl border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-900 outline-none focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/30',
          className,
        )}
        {...rest}
      />
    </label>
  )
}

export function Alert({ type, children }: { type: 'ok' | 'err' | 'info'; children: ReactNode }) {
  const c =
    type === 'ok'
      ? 'border-emerald-200 bg-emerald-50 text-emerald-900'
      : type === 'err'
        ? 'border-rose-200 bg-rose-50 text-rose-900'
        : 'border-indigo-200 bg-indigo-50 text-indigo-900'
  return <div className={`rounded-xl border px-4 py-3 text-sm ${c}`}>{children}</div>
}

export function Spinner({ className = '' }: { className?: string }) {
  return (
    <div
      className={`h-5 w-5 animate-spin rounded-full border-2 border-indigo-200 border-t-indigo-600 ${className}`}
      aria-hidden
    />
  )
}

/** Base pulse block for loading placeholders */
export function Skeleton({ className = '' }: { className?: string }) {
  return <div className={twMerge('animate-pulse rounded-lg bg-slate-200/90', className)} aria-hidden />
}

export function TableSkeleton({ rows = 6, columns = 5 }: { rows?: number; columns?: number }) {
  return (
    <div className="overflow-x-auto" role="status" aria-label="Loading table">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-200">
            {Array.from({ length: columns }).map((_, i) => (
              <th key={i} className="pb-3 pr-4">
                <Skeleton className="h-4 w-24" />
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {Array.from({ length: rows }).map((_, ri) => (
            <tr key={ri} className="border-b border-slate-100">
              {Array.from({ length: columns }).map((_, ci) => (
                <td key={ci} className="py-3 pr-4">
                  <Skeleton className="h-4 w-full max-w-[12rem]" />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/** Stacked rows resembling notification / list cards */
export function ListSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <ul className="space-y-3" role="status" aria-label="Loading list">
      {Array.from({ length: rows }).map((_, i) => (
        <li key={i} className="rounded-xl border border-slate-100 px-4 py-3">
          <Skeleton className="mb-2 h-4 w-48" />
          <Skeleton className="mb-2 h-3 w-full max-w-xl" />
          <Skeleton className="h-3 w-32" />
        </li>
      ))}
    </ul>
  )
}

/** Tree / bullet list placeholder */
export function TreeSkeleton({ depth = 3 }: { depth?: number }) {
  return (
    <div className="space-y-2" role="status" aria-label="Loading hierarchy">
      {Array.from({ length: depth }).map((_, i) => (
        <div key={i} style={{ marginLeft: i * 12 }}>
          <Skeleton className="h-4 w-56" />
        </div>
      ))}
    </div>
  )
}

/** Dashboard metric cards grid */
export function DashboardGridSkeleton({ cards = 5 }: { cards?: number }) {
  return (
    <div className="grid gap-6 lg:grid-cols-2" role="status" aria-label="Loading dashboard">
      {Array.from({ length: cards }).map((_, i) => (
        <Card key={i}>
          <Skeleton className="mb-4 h-4 w-28" />
          <div className="space-y-2">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-[85%]" />
            <Skeleton className="h-4 w-[60%]" />
          </div>
        </Card>
      ))}
    </div>
  )
}

/** Employee detail two-column layout */
export function DetailPageSkeleton() {
  return (
    <div className="grid gap-8 lg:grid-cols-2" role="status" aria-label="Loading profile">
      <Card>
        <Skeleton className="mb-4 h-4 w-24" />
        <div className="space-y-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="flex justify-between gap-4">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 flex-1 max-w-xs" />
            </div>
          ))}
        </div>
      </Card>
      <Card>
        <Skeleton className="mb-4 h-4 w-32" />
        <div className="space-y-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      </Card>
    </div>
  )
}

export function PageTitle({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mb-8">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">{title}</h1>
      {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
    </div>
  )
}
