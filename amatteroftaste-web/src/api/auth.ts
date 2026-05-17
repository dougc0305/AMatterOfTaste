import { api } from './client';
import type { LoginResponse } from '../types';

export function login(email: string, password: string) {
  return api.post<LoginResponse>('/auth/login', { email, password });
}

export function register(name: string, email: string, password: string) {
  return api.post<LoginResponse>('/auth/register', { name, email, password });
}

export function changePassword(currentPassword: string, newPassword: string) {
  return api.put<{ message: string }>('/auth/change-password', { currentPassword, newPassword });
}
