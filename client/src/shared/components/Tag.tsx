import type { ReactNode } from 'react'

type TagVariant = 'accent' | 'sage' | 'sand' | 'outline'

const VARIANTS: Record<TagVariant, string> = {
  accent: 'bg-accent-100 text-accent-800',
  sage: 'bg-sage-100 text-sage-800',
  sand: 'bg-sand-100 text-sand-800',
  outline: 'border border-accent-700 text-accent-700',
}

interface TagProps {
  variant?: TagVariant
  className?: string
  children: ReactNode
}

/** Small pill label — event status, roles, invite state. Matches the maquette's `.tag` classes. */
export function Tag({ variant = 'sand', className = '', children }: TagProps) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-1 text-[11px] font-medium tracking-wide whitespace-nowrap ${VARIANTS[variant]} ${className}`}
    >
      {children}
    </span>
  )
}
