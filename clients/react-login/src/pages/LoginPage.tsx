import { useId, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/// CONTROLLED COMPONENTS: `username`/`password` live in React state, and each <input>'s `value`
/// is driven FROM that state with `onChange` writing back to it -- React is the single source of
/// truth for what's on screen, as opposed to an "uncontrolled" input you'd read via a ref only on
/// submit. Controlled is the conventional default; uncontrolled trades that control away for
/// marginally less re-rendering on very large forms.
export function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  // useId: generates a stable, unique id per component instance for the label/input `for`/`id`
  // pairing -- safe under React 18+ concurrent rendering and server-side rendering (a plain
  // hardcoded string id would collide if this component ever rendered twice on one page).
  const usernameId = useId();
  const passwordId = useId();

  async function handleSubmit(event: FormEvent) {
    event.preventDefault(); // the one line every "explain controlled forms" answer must mention
    setIsLoading(true);
    setError(null);

    const loginError = await login(username, password);

    setIsLoading(false);
    if (loginError) {
      setError(loginError);
    } else {
      navigate('/members');
    }
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1>InterviewPrep Login</h1>
        <p className="hint">
          Try <code>admin / Admin123!</code> or <code>staff / Staff123!</code>
        </p>

        <label htmlFor={usernameId}>Username</label>
        <input
          id={usernameId}
          type="text"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          autoComplete="username"
          required
        />

        <label htmlFor={passwordId}>Password</label>
        <input
          id={passwordId}
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
          minLength={4}
        />

        {error && <p className="form-error" role="alert">{error}</p>}

        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
