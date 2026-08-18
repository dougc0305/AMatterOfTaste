const API_BASE = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem('token');
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options?.headers,
  };

  const response = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (!response.ok) {
    let body: { message?: string } | null = null;
    try { body = await response.json(); } catch { /* non-JSON body */ }

    // A 401 on a protected endpoint almost always means the JWT expired (they
    // last 7 days). The server sends a bare 401 with no body, so without this
    // callers report their own generic failure ("couldn't parse the recipe"),
    // which points at the wrong problem. /auth/ is excluded because
    // /auth/login returns 401 for bad credentials and its own message
    // ("Invalid email or password") is the correct one to show.
    const sessionExpired = response.status === 401 && !path.startsWith('/auth/');

    const err = new Error(
      sessionExpired
        ? 'Your session has expired. Please log in again.'
        : body?.message ?? `API error: ${response.status}`
    ) as Error & {
      status: number;
      body: { message?: string } | null;
      sessionExpired: boolean;
    };
    err.status = response.status;
    err.body = body;
    err.sessionExpired = sessionExpired;
    throw err;
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
};
