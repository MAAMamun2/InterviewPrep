// INTERFACE vs TYPE: this is an `interface` because it describes the SHAPE of a value coming off
// the wire (the API response) -- interfaces are the conventional choice for object shapes that
// might later be extended/implemented. Contrast with the discriminated union below, which uses
// `type` because a union of literal variants isn't expressible as an interface at all.
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: 'Admin' | 'Staff';
}

// DISCRIMINATED UNION: three possible shapes of "where is the login attempt right now", and
// TypeScript narrows `state.error`/`state.response` correctly inside each `if (state.status === ...)`
// branch -- you cannot accidentally read `.error` while status is 'success'. The direct analogue
// of the C# Result<T> pattern in the backend (Application/Common/Result.cs), just expressed as a
// union instead of a class hierarchy -- a good "same idea, different language" talking point.
export type AuthState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; response: LoginResponse }
  | { status: 'error'; error: string };
