import { type ChangeEvent, useRef } from 'react'
import { btnGhost, btnSecondary, fieldLabel } from '../../../shared/lib/ui'
import { prepareEventImage } from '../../../shared/lib/image'

export interface SelectedEventImage {
  /** Image déjà réduite (cf. prepareEventImage), envoyée telle quelle à l'API. */
  blob: Blob
  previewUrl: string
}

interface EventImageFieldProps {
  image: SelectedEventImage | null
  onChange: (image: SelectedEventImage | null) => void
  isPreparing: boolean
  onPreparingChange: (isPreparing: boolean) => void
  disabled?: boolean
}

// Data URL plutôt que URL.createObjectURL : rien à révoquer au démontage, et disponible sous jsdom.
function readAsDataUrl(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = () => reject(reader.error)
    reader.readAsDataURL(blob)
  })
}

export function EventImageField({ image, onChange, isPreparing, onPreparingChange, disabled }: EventImageFieldProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)

  async function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    // Réinitialisé pour qu'un nouveau choix du même fichier (ex: après une erreur) redéclenche onChange.
    event.target.value = ''
    if (!file) return

    onPreparingChange(true)
    try {
      const blob = await prepareEventImage(file)
      onChange({ blob, previewUrl: await readAsDataUrl(blob) })
    } finally {
      onPreparingChange(false)
    }
  }

  const isDisabled = disabled || isPreparing

  return (
    <div className="flex flex-col gap-1.5">
      <span className={fieldLabel}>
        Image <span className="text-ink/50">— optionnel</span>
      </span>

      {image && (
        <div className="h-40 overflow-hidden rounded-3xl">
          <img src={image.previewUrl} alt="" data-testid="create-event-image-preview" className="h-full w-full object-cover" />
        </div>
      )}

      <input
        ref={fileInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        onChange={handleFileChange}
        data-testid="create-event-image-input"
        className="sr-only"
        tabIndex={-1}
        aria-hidden="true"
      />
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={isDisabled}
          data-testid="create-event-image-choose-button"
          className={`${btnSecondary} min-h-10 px-4 text-[14px]`}
        >
          {isPreparing ? 'Préparation…' : image ? "Changer l'image" : 'Choisir une image'}
        </button>
        {image && (
          <button
            type="button"
            onClick={() => onChange(null)}
            disabled={isDisabled}
            data-testid="create-event-image-remove-button"
            className={`${btnGhost} min-h-9 px-2 text-[13px]`}
          >
            Retirer l'image
          </button>
        )}
      </div>
    </div>
  )
}
