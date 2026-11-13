// « Jeudi 24 décembre · 19:30 », en heure locale.
export function formatEventDate(iso: string): string {
  const date = new Date(iso)
  const day = date.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
  const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  return `${day.charAt(0).toUpperCase()}${day.slice(1)} · ${time}`
}
