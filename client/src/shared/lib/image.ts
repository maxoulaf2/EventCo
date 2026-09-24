const AVATAR_SIZE = 512
const EVENT_IMAGE_MAX_SIDE = 1600
const JPEG_QUALITY = 0.85

interface SourceRect {
  x: number
  y: number
  width: number
  height: number
}

/**
 * Recadre au centre (carré) et réduit une photo à 512×512 px en JPEG avant envoi : une photo de
 * smartphone (plusieurs Mo) devient quelques dizaines de Ko, bien sous la limite de 2 Mo de l'API
 * et sans traitement d'image côté serveur.
 *
 * Si le navigateur ne sait pas décoder le fichier (format exotique, ou environnement sans canvas comme
 * jsdom en test), le fichier d'origine est envoyé tel quel : l'API reste seule juge du format accepté
 * et renvoie une erreur explicite le cas échéant.
 */
export function prepareAvatarImage(file: File): Promise<Blob> {
  return renderAsJpeg(file, (bitmap) => {
    const side = Math.min(bitmap.width, bitmap.height)
    const size = Math.min(side, AVATAR_SIZE)
    return {
      source: { x: (bitmap.width - side) / 2, y: (bitmap.height - side) / 2, width: side, height: side },
      width: size,
      height: size,
    }
  })
}

/**
 * Réduit l'image de présentation d'un événement à 1600 px sur son plus grand côté, sans recadrage
 * (l'affichage la cadre en bandeau), en JPEG : quelques centaines de Ko, sous la limite de 5 Mo de
 * l'API. Même repli que prepareAvatarImage si le navigateur ne sait pas décoder le fichier.
 */
export function prepareEventImage(file: File): Promise<Blob> {
  return renderAsJpeg(file, (bitmap) => {
    const scale = Math.min(1, EVENT_IMAGE_MAX_SIDE / Math.max(bitmap.width, bitmap.height))
    return {
      source: { x: 0, y: 0, width: bitmap.width, height: bitmap.height },
      width: Math.round(bitmap.width * scale),
      height: Math.round(bitmap.height * scale),
    }
  })
}

async function renderAsJpeg(
  file: File,
  layout: (bitmap: ImageBitmap) => { source: SourceRect; width: number; height: number },
): Promise<Blob> {
  try {
    const bitmap = await createImageBitmap(file)
    const { source, width, height } = layout(bitmap)

    const canvas = document.createElement('canvas')
    canvas.width = width
    canvas.height = height
    const context = canvas.getContext('2d')
    if (!context) return file

    // JPEG n'a pas de transparence : sans fond, les zones transparentes d'un PNG deviendraient noires.
    context.fillStyle = '#ffffff'
    context.fillRect(0, 0, width, height)
    context.drawImage(bitmap, source.x, source.y, source.width, source.height, 0, 0, width, height)
    bitmap.close()

    const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, 'image/jpeg', JPEG_QUALITY))
    return blob ?? file
  } catch {
    return file
  }
}
