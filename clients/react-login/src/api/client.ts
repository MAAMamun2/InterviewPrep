import type { ApiResult } from '../types';

const API_BASE_URL = 'http://localhost:5000'; // direct cross-origin call -- exercises real CORS,
// unlike the Angular client which proxies through the dev server. Deliberately different so both
// approaches are demonstrated across the two frontends: ask "why does React need CORS configured
// on the backend but Angular's demo didn't" and the proxy.conf.json vs. this file is the answer.

let authToken: string | null = null;
export function setAuthToken(token: string | null) {
  authToken = token;
}

// GENERIC FUNCTION: <T> is inferred from how the caller uses the result (see login()/getMembers()
// below) -- one function, reused for every endpoint's response shape, fully type-checked at each
// call site. This is the TypeScript analogue of generic repository methods in the C# backend.
export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<ApiResult<T>> {
  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
        ...options.headers,
      },
    });

    if (!response.ok) {
      const body = await response.json().catch(() => null);
      return { status: 'error', message: body?.error ?? body?.title ?? `Request failed (${response.status})` };
    }

    const data = (await response.json()) as T;
    return { status: 'success', data };
  } catch {
    return { status: 'error', message: 'Network error -- is the API running on :5000?' };
  }
}
