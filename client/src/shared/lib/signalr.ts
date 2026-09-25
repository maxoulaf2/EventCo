import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { API_BASE_URL } from './api'
import { checkAppVersion } from './appVersionWatch'

export function createEventHubConnection(): HubConnection {
  // Le client SignalR ne sait résoudre une URL relative que via le DOM d'un vrai navigateur (cf.
  // HttpConnection._resolveUrl, qui exige Platform.isBrowser) : on résout nous-mêmes l'origine
  // courante quand API_BASE_URL est vide (cas par défaut, proxifié par Vite en dev).
  const baseUrl = API_BASE_URL || window.location.origin
  const connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/events`, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  // Un déploiement coupe forcément la connexion temps réel : la reconnexion est le bon moment pour
  // vérifier que le frontend tourne toujours dans la même version que l'API.
  connection.onreconnected(() => void checkAppVersion())

  return connection
}
