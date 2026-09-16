import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { API_BASE_URL } from './api'

export function createEventHubConnection(): HubConnection {
  // Le client SignalR ne sait résoudre une URL relative que via le DOM d'un vrai navigateur (cf.
  // HttpConnection._resolveUrl, qui exige Platform.isBrowser) : on résout nous-mêmes l'origine
  // courante quand API_BASE_URL est vide (cas par défaut, proxifié par Vite en dev).
  const baseUrl = API_BASE_URL || window.location.origin
  return new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/events`, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
