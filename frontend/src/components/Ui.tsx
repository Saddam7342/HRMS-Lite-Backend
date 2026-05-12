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

export function PageTitle({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mb-8">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">{title}</h1>
      {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
    </div>
  )
}
