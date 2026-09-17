/**
 * Shared Tailwind class strings for the "Lemon" look — kept here rather than repeated inline
 * across every form/page, per docs/conventions-code.md §2.2 (Tailwind exclusively, no separate
 * component CSS). Sizes/spacing per-usage stay inline; only the recurring color/shape rules live here.
 */
export const btnPrimary =
  'inline-flex min-h-11 items-center justify-center gap-2 rounded-full bg-accent-500 px-6 font-heading text-[15px] text-accent-900 transition-colors hover:bg-accent-400 active:bg-accent-600 disabled:cursor-not-allowed disabled:opacity-45'

export const btnSecondary =
  'inline-flex min-h-11 items-center justify-center gap-2 rounded-full border border-border px-6 font-heading text-[15px] text-ink transition-colors hover:bg-ink/5 active:bg-ink/10 disabled:cursor-not-allowed disabled:opacity-45'

export const btnGhost =
  'inline-flex min-h-11 items-center justify-center gap-2 rounded-full px-3 font-heading text-[15px] text-accent-700 transition-colors hover:bg-accent-100 active:bg-accent-200 disabled:cursor-not-allowed disabled:opacity-45'

export const btnIcon =
  'inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-bg text-ink shadow-elev-sm transition-colors hover:bg-sand-100 active:bg-sand-200'

export const fieldLabel = 'block text-[13px] text-ink/70'

export const input =
  'min-h-14 w-full rounded-full border border-border bg-sand-100 px-[18px] text-[16px] text-ink placeholder:text-ink/45 focus-visible:border-accent-600'

export const textarea =
  'min-h-[72px] w-full rounded-2xl border border-border bg-sand-100 px-[18px] py-3 text-[16px] text-ink placeholder:text-ink/45 focus-visible:border-accent-600'

export const cardSurface = 'rounded-[calc(var(--radius-2xl)*1.15)] bg-surface shadow-elev-sm'
