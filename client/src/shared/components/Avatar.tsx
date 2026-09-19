import { useState } from 'react'

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

/** Deterministic initial + palette color from a display name, matching the maquette's avatar circles.
 * Shows the full name in a tooltip on mouse hover or on touch-and-hold (mobile has no hover state). */
export function Avatar({ name, size = 'md', ringed = false, className = '' }: AvatarProps) {
  const [showTooltip, setShowTooltip] = useState(false)
  const initial = name.trim().charAt(0).toUpperCase() || '?'
  const color = PALETTE[hash(name) % PALETTE.length]

  return (
    <span
      className={`relative inline-grid shrink-0 select-none place-items-center ${showTooltip ? 'z-20' : ''}`}
      style={{ WebkitUserSelect: 'none', WebkitTouchCallout: 'none' }}
      onMouseEnter={() => setShowTooltip(true)}
      onMouseLeave={() => setShowTooltip(false)}
      onTouchStart={() => setShowTooltip(true)}
      onTouchEnd={() => setShowTooltip(false)}
      onTouchCancel={() => setShowTooltip(false)}
    >
      <span
        aria-hidden="true"
        className={`inline-grid place-items-center rounded-full font-semibold text-bg ${SIZES[size]} ${color} ${ringed ? 'shadow-[0_0_0_2.5px_var(--color-surface)]' : ''} ${className}`}
      >
        {initial}
      </span>
      {showTooltip && (
        <span
          role="tooltip"
          className="pointer-events-none absolute bottom-full left-1/2 z-30 mb-1.5 -translate-x-1/2 whitespace-nowrap rounded-md bg-ink px-2 py-1 text-[11px] font-medium text-bg shadow-md"
        >
          {name}
        </span>
      )}
    </span>
  )
}
