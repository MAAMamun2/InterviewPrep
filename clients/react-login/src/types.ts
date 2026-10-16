// INTERFACE for the wire-shape of API responses (same reasoning as the Angular client's auth.model.ts:
// interfaces for object shapes, `type` for the union below).
export interface LoginResponse {
  token: string;
  username: string;
  role: 'Admin' | 'Staff';
}

export interface Member {
  id: number;
  name: string;
  age: number;
  gender: string;
  tier: string;
  purchaseCount: number;
}

// DISCRIMINATED UNION, the TypeScript-native version of the backend's Result<T> (Application/Common/Result.cs)
// and identical in spirit to the Angular AuthState union -- same problem, three languages, three
// idiomatic shapes. `status` is the discriminant TypeScript narrows on.
export type ApiResult<T> =
  | { status: 'success'; data: T }
  | { status: 'error'; message: string };
