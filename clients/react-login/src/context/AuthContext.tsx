import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { apiFetch, setAuthToken } from '../api/client';
import type { LoginResponse } from '../types';

const TOKEN_STORAGE_KEY = 'interviewprep.react.token';

interface AuthContextValue {
  username: string | null;
  role: 'Admin' | 'Staff' | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<string | null>; // returns an error message, or null on success
  logout: () => void;
}

// CONTEXT API: avoids "prop drilling" auth state through every component between the router root
// and whatever page needs it. The tradeoff worth naming unprompted: Context causes every consumer
// to re-render on ANY value change (no fine-grained subscriptions like a real state library would
// give you) -- fine at this scale, a real consideration in a bigger app (that's where Redux/Zustand/
// Jotai earn their keep).
const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  // useState, not useRef: token changes must trigger a re-render (e.g. to flip the router between
  // login and the protected page), which useRef deliberately does NOT do.
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_STORAGE_KEY));
  const [username, setUsername] = useState<string | null>(null);
  const [role, setRole] = useState<'Admin' | 'Staff' | null>(null);

  if (token) setAuthToken(token); // keep the plain api client module in sync with React state

  async function login(usernameInput: string, password: string): Promise<string | null> {
    const result = await apiFetch<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username: usernameInput, password }),
    });

    if (result.status === 'error') {
      return result.message; // TypeScript knows `.message` exists only on this branch
    }

    setToken(result.data.token);
    setUsername(result.data.username);
    setRole(result.data.role);
    setAuthToken(result.data.token);
    localStorage.setItem(TOKEN_STORAGE_KEY, result.data.token);
    return null;
  }

  function logout() {
    setToken(null);
    setUsername(null);
    setRole(null);
    setAuthToken(null);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }

  // useMemo: without it, this object literal is a BRAND NEW reference every render, which would
  // make every consumer of useAuth() re-render even when nothing meaningful changed (and would
  // break React.memo on any child that takes this value as a prop). Recomputes only when one of
  // the dependencies actually changes.
  const value = useMemo<AuthContextValue>(
    () => ({ username, role, isAuthenticated: token !== null, login, logout }),
    [username, role, token]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// CUSTOM HOOK: wraps useContext + a null-check in one place, so every consumer gets a typed,
// non-null value and a clear error instead of a silent `null` if used outside the provider.
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
