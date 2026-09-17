const PALETTE = [
  'bg-sage-600',
  'bg-accent-800',
  'bg-sand-600',
  'bg-sand-500',
  'bg-accent-600',
  'bg-sage-700',
  'bg-sand-700',
]

function hash(input: string): number {
  let h = 0
  for (let i = 0; i < input.length; i++) {
    h = (h << 5) - h + input.charCodeAt(i)
    h |= 0
  }
  return Math.abs(h)
}

interface AvatarProps {
  name: string
  size?: 'sm' | 'md' | 'lg'
  ringed?: boolean
  className?: string
}

const SIZES = {
  sm: 'h-[30px] w-[30px] text-[11px]',
  md: 'h-[38px] w-[38px] text-[12px]',
  lg: 'h-11 w-11 text-[15px]',
}

/** Deterministic initial + palette color from a display name, matching the maquette's avatar circles. */
export function Avatar({ name, size = 'md', ringed = false, className = '' }: AvatarProps) {
  const initial = name.trim().charAt(0).toUpperCase() || '?'
  const color = PALETTE[hash(name) % PALETTE.length]

  return (
    <span
      aria-hidden="true"
      className={`inline-grid shrink-0 place-items-center rounded-full font-semibold text-bg ${SIZES[size]} ${color} ${ringed ? 'shadow-[0_0_0_2.5px_var(--color-surface)]' : ''} ${className}`}
    >
      {initial}
    </span>
  )
}
