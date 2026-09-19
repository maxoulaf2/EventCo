/**
 * `navigator.clipboard` n'existe que dans un contexte sécurisé (HTTPS ou `localhost`) — absent
 * en HTTP sur le réseau local (ex: test mobile via l'IP LAN du poste de dev) ou sur les
 * navigateurs plus anciens. Fallback `execCommand('copy')` (déprécié mais toujours largement
 * supporté) dans ces cas plutôt que de laisser l'appel échouer silencieusement.
 */
export async function copyTextToClipboard(text: string): Promise<boolean> {
  if (navigator.clipboard && window.isSecureContext) {
    try {
      await navigator.clipboard.writeText(text)
      return true
    } catch {
      // Retente via le fallback ci-dessous plutôt que d'abandonner.
    }
  }

  const textarea = document.createElement('textarea')
  textarea.value = text
  textarea.style.position = 'fixed'
  textarea.style.opacity = '0'
  document.body.appendChild(textarea)
  textarea.focus()
  textarea.select()

  let succeeded = false
  try {
    succeeded = document.execCommand('copy')
  } catch {
    succeeded = false
  }

  document.body.removeChild(textarea)
  return succeeded
}
