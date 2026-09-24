const AVATAR_SIZE = 512
const AVATAR_JPEG_QUALITY = 0.85

/**
 * Recadre au centre (carré) et réduit une photo à 512×512 px en JPEG avant envoi : une photo de
 * smartphone (plusieurs Mo) devient quelques dizaines de Ko, bien sous la limite de 2 Mo de l'API
 * et sans traitement d'image côté serveur.
 *
 * Si le navigateur ne sait pas décoder le fichier (format exotique, ou environnement sans canvas comme
 * jsdom en test), le fichier d'origine est envoyé tel quel : l'API reste seule juge du format accepté
 * et renvoie une erreur explicite le cas échéant.
 */
export async function prepareAvatarImage(file: File): Promise<Blob> {
  try {
    const bitmap = await createImageBitmap(file)
    const side = Math.min(bitmap.width, bitmap.height)
    const size = Math.min(side, AVATAR_SIZE)

    const canvas = document.createElement('canvas')
    canvas.width = size
    canvas.height = size
    const context = canvas.getContext('2d')
    if (!context) return file

    // JPEG n'a pas de transparence : sans fond, les zones transparentes d'un PNG deviendraient noires.
    context.fillStyle = '#ffffff'
    context.fillRect(0, 0, size, size)
    context.drawImage(bitmap, (bitmap.width - side) / 2, (bitmap.height - side) / 2, side, side, 0, 0, size, size)
    bitmap.close()

    const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, 'image/jpeg', AVATAR_JPEG_QUALITY))
    return blob ?? file
  } catch {
    return file
  }
}
