import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/// The React equivalent of Angular's authGuard: a wrapper component instead of a router config
/// callback (React Router has no built-in guard concept -- "protect a route" means "render
/// something else instead" in idiomatic React). <Navigate> performs a client-side redirect.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
