import { api } from './client';

export interface VisitDay {
  date: string;
  count: number;
}

export function pingVisit() {
  return api.post<void>('/visits/ping', {});
}

export function getVisitDays(days = 90) {
  return api.get<VisitDay[]>(`/visits?days=${days}`);
}
