import { useState, useEffect, useCallback } from 'react';
import type { User } from '../types';

/**
 * True once the JWT's own `exp` claim has passed. Without this check the app
 * trusts the cached user object, so an expired session still renders the admin
 * page while every API call it makes comes back 401. This is a UX guard, not a
 * security control — the server validates the token on every request.
 */
function isExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return typeof payload.exp === 'number' && payload.exp * 1000 <= Date.now();
  } catch {
    return true; // unparseable token is unusable
  }
}

export function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const saved = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    if (saved && savedUser && !isExpired(saved)) {
      setToken(saved);
      setUser(JSON.parse(savedUser));
    } else if (saved || savedUser) {
      // Expired or half-written session — clear it so the UI reflects reality.
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  }, []);

  const saveAuth = useCallback((token: string, user: User) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    setToken(token);
    setUser(user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
  }, []);

  return { user, token, isLoggedIn: !!token, isAdmin: user?.isAdmin ?? false, saveAuth, logout };
}
