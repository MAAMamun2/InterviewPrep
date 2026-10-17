# React — Basics to Advanced, All in One File

Single consolidated reference for the React client (`clients/react-login`), ordered basic → advanced.
Everything below is backed by a real file in this repo — open it, read the comment block at the top
(that's where the "why" lives), then come back here for the explanation and the one-liner to say out
loud in an interview. This file exists alongside `docs/INTERVIEW-MAP.md` (which spreads the same
topics across C#/Angular/SQL too) — use that one for the full-stack picture, this one when you just
want to drill React start to finish.

---

## 1. Fundamentals

**Component** — a function that returns UI. `LoginPage`, `MembersPage`, `ProtectedRoute` are all just
JavaScript functions.

**JSX** — the `<div>`/`<input>` syntax inside those functions. Looks like HTML, compiles to
`React.createElement(...)` calls that build a description of the UI, not real DOM nodes yet.

**Props vs. State**
- **Props**: data passed *into* a component by its parent (e.g. `<ProtectedRoute>{children}</ProtectedRoute>`
  — `children` is a prop).
- **State**: data a component owns itself, that changes over time and triggers a re-render when it
  does (`const [search, setSearch] = useState('')` in `MembersPage.tsx:10`).

**The core render loop**: state changes → component function re-runs → returns new JSX → React diffs
the new tree against the previous one (**reconciliation**, using a lightweight in-memory "virtual DOM")
→ only the real DOM nodes that actually changed get touched. This is *why* typing in one `<input>`
doesn't repaint the whole page.

**The Rules of Hooks** (know this cold, it's asked constantly): hooks — any function starting with
`use` — can only be called at the top level of a component or another hook, never inside an `if`,
loop, or nested function. React tracks hooks by *call order* across renders; a conditional hook call
breaks that order and corrupts state on the next render.

---

## 2. Hooks, in the order this project uses them

### `useState` — `LoginPage.tsx:11-14`, `MembersPage.tsx:9-10`
Holds a value and a setter. Calling the setter schedules a re-render with the new value. Nothing
exotic — every other hook below builds on this one.

### `useEffect` with cleanup — `useDebouncedValue.ts` (the clearest example in the project)
```ts
useEffect(() => {
  const timer = setTimeout(() => setDebounced(value), delayMs);
  return () => clearTimeout(timer); // cleanup runs before the NEXT effect, or on unmount
}, [value, delayMs]);
```
This **is** the "write a debounce hook" interview exercise. `useState` holds the delayed value;
`useEffect` delays setting it. The returned cleanup function cancels the *previous* timer every time
`value` changes again before it fires — that cancellation is the entire mechanism of debouncing.
Without `return () => clearTimeout(timer)`, every keystroke would schedule a timer that still fires,
and you'd see every intermediate value applied instead of just the final one.

**Dependency array mental model**: `[value, delayMs]` means "re-run this effect only when `value` or
`delayMs` changes." An empty array `[]` means "run once on mount, never again." No array at all means
"run after every render" (almost never what you want).

### `useEffect` for data fetching + the stale-response bug — `MembersPage.tsx:21-36`
```ts
useEffect(() => {
  let cancelled = false;
  async function load() {
    const result = await apiFetch<Member[]>(`/api/members${query}`);
    if (!cancelled && result.status === 'success') setMembers(result.data);
  }
  load();
  return () => { cancelled = true; };
}, [debouncedSearch]);
```
**The bug this prevents**: type "al", then quickly type "alice". Two requests fire. If the "al"
request resolves *after* the "alice" request (networks don't guarantee ordering), and you don't guard
against it, the stale "al" results overwrite the correct "alice" results on screen. The `cancelled`
flag — set `true` in the cleanup, which runs when the effect re-fires on the next keystroke — means
the stale request's result is silently dropped. A production app would use `AbortController` instead
(actually cancels the network request, not just ignores its result) — know that `cancelled`-flag and
`AbortController` are the two standard answers to this exact question, and `AbortController` is the
better one.

### Custom hooks — `useDebouncedValue.ts` (the hook itself) + `AuthContext.tsx`'s `useAuth()`
A custom hook is just a regular function, by convention named `use...`, that calls other hooks inside
it. `useDebouncedValue<T>(value: T, delayMs: number): T` is generic — reusable for a string search box
today, a number or object tomorrow, no duplication. `useAuth()` (`AuthContext.tsx:71-77`) is a second
flavor: it wraps `useContext` plus a null-check in one place, so every consumer gets a typed, non-null
value and a clear thrown error instead of a silent `null` if someone calls it outside the provider.

### `useId` — `LoginPage.tsx:22-23`
Generates a stable, unique id per component *instance*, used to pair `<label htmlFor={id}>` with
`<input id={id}>`. A hardcoded string id would collide if the component ever rendered twice on the
same page (a modal + a full page, server-side rendering). `useId()` guarantees uniqueness without you
managing it.

### `useMemo` — `AuthContext.tsx:61-64`
```ts
const value = useMemo<AuthContextValue>(
  () => ({ username, role, isAuthenticated: token !== null, login, logout }),
  [username, role, token]
);
```
Without `useMemo`, this object literal is a **brand-new reference every render** — even if
`username`/`role`/`token` haven't changed. Every consumer of `useAuth()` would re-render on *any*
re-render of `AuthProvider`, and it would defeat `React.memo` on any child receiving this object as a
prop (memo compares references). `useMemo` recomputes the object only when a listed dependency
actually changes. This is the single most common "why would you use `useMemo` here specifically"
interview question, and this file is a real, provable answer instead of a textbook one.

---

## 3. Component patterns

### Controlled components — `LoginPage.tsx:49-67`
`<input value={username} onChange={(e) => setUsername(e.target.value)} />` — the input's value is
*driven from* React state, and `onChange` writes *back* into that state. React is the single source
of truth; the DOM input never has an independent value. The alternative, **uncontrolled**, skips
`value`/`onChange` and reads a `ref` only on submit — no per-keystroke re-render, but you give up
real-time validation/transformation. Controlled is the default; uncontrolled is an optimization for
very large forms re-rendering too often.

Two details worth having ready: `event.preventDefault()` on submit (without it, a native form submit
does a full page reload and wipes all React state), and `required`/`minLength` on the inputs being
**native HTML validation** — UX only, never a substitute for real server-side validation (the backend's
FluentValidation is the actual enforcement).

### Context API — `AuthContext.tsx:20, 22-67`
Solves **prop drilling**: without it, `username`/`role`/`login`/`logout` would have to be passed as
props through every component between the router root and whatever page needs them. `createContext`
+ a `Provider` makes the value available to any descendant via `useContext` (wrapped here as
`useAuth()`), no matter how deep.

**The tradeoff to name unprompted**: every consumer of a context re-renders on *any* change to the
context value — there's no fine-grained subscription like a real state-management library gives you.
Fine at this scale (one auth object); a real consideration once an app has many independent pieces of
shared state, which is exactly where Redux/Zustand/Jotai earn their keep over plain Context.

**`useState` not `useRef` for the token** (`AuthContext.tsx:25`): token changes must trigger a
re-render (to flip the router between the login page and the protected page). `useRef` deliberately
does *not* trigger a re-render on mutation — the wrong tool here, useful elsewhere (e.g. holding the
debounce `timer` id, a DOM node reference, or any value you want to persist across renders without
causing one).

### Route protection — `ProtectedRoute.tsx`
React Router has no built-in "guard" concept the way Angular does (`core/guards/auth.guard.ts` there
is a function the router config calls). In idiomatic React, "protect a route" means **render
something else instead** — a wrapper component that checks `useAuth().isAuthenticated` and returns
`<Navigate to="/login" replace />` if false, or `children` if true. Same problem, different idiom:
Angular's router asks a guard function "can I activate this route?"; React just renders a redirect
component in the route's place.

---

## 4. Lists & reconciliation

### The `key` prop — `MembersPage.tsx:76`
```tsx
members.map((member) => (
  <tr key={member.id}>...</tr>
))
```
React's reconciliation needs a **stable, unique key per list item** to correctly diff/reorder/preserve
state across re-renders — it uses the key to match old DOM nodes to new ones. **The classic
antipattern**: using the array *index* as the key. That breaks the moment items are inserted, removed,
or reordered, because the index-to-item mapping shifts but React still matches by the old index,
attaching the wrong state (or DOM focus, or form input value) to the wrong item. Always key by a
stable identity from the data (`member.id`), never by position.

---

## 5. TypeScript + React

### Generics — `api/client.ts:16` (`apiFetch<T>`)
```ts
export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<ApiResult<T>> {
```
One function, reused for every endpoint's response shape (`apiFetch<LoginResponse>(...)`,
`apiFetch<Member[]>(...)`), fully type-checked at each call site — `T` is inferred from how the
caller uses the result. This is the direct TypeScript analogue of a generic repository method on the
C# backend (`IMemberRepository`), or Angular's typed `HttpClient.get<T>(...)` calls.

### Discriminated unions — `types.ts:21-23` (`ApiResult<T>`)
```ts
export type ApiResult<T> =
  | { status: 'success'; data: T }
  | { status: 'error'; message: string };
```
`status` is the **discriminant** — the field TypeScript narrows on. Inside `if (result.status ===
'error') return result.message;`, TypeScript knows `.message` exists *only* on that branch (and
knows `.data` exists only on the `'success'` branch) — no manual type assertion needed. This mirrors
the backend's `Result<T>` (`Application/Common/Result.cs`) and Angular's `AuthState` union exactly —
same problem, three languages, three idiomatic versions of the same shape. A great "show me you think
across the stack" talking point.

`interface` vs. `type`: this codebase uses `interface` for object shapes (`LoginResponse`, `Member`)
and `type` for the union (`ApiResult<T>`) — the conventional split, since unions can't be expressed as
an `interface`.

---

## 6. Wiring it together — `App.tsx`
```tsx
<AuthProvider>
  <BrowserRouter>
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/members" element={<ProtectedRoute><MembersPage /></ProtectedRoute>} />
    </Routes>
  </BrowserRouter>
</AuthProvider>
```
`AuthProvider` wraps the router (not the other way around) so every page, including `ProtectedRoute`
itself, can call `useAuth()`. This is the one place to look to see the whole app's shape at a glance.

---

## 7. One cross-cutting detail: CORS — `api/client.ts:3-6`
The React client calls the backend **directly, cross-origin** (`http://localhost:5000` from a page
served on `:5173`) — no dev proxy, unlike the Angular client (`proxy.conf.json`). That's deliberate:
it's why the backend's `Program.cs` needs an explicit CORS policy at all. "Why does the React client
need CORS configured on the backend but the Angular demo doesn't" — the proxy vs. this file is the
exact answer: Angular's dev server makes the request look same-origin to the browser; React's doesn't.

---

## Quick-fire Q&A

| Question | One-liner |
|---|---|
| Controlled vs. uncontrolled component? | Controlled: value lives in React state, `value`+`onChange`. Uncontrolled: read via `ref` on submit, no per-keystroke re-render. |
| Why `useMemo` on the context value? | New object reference every render otherwise → every consumer re-renders unnecessarily, breaks `React.memo` on children. |
| Why `useState` not `useRef` for the token? | State change must trigger a re-render (switch between login/protected views); `useRef` mutation doesn't. |
| The stale-response bug? | A slower earlier request resolving after a newer one overwrites fresher data; guard with a `cancelled` flag or `AbortController`. |
| Why never use array index as `key`? | Breaks identity tracking the moment the list is reordered/inserted/removed — React matches old DOM/state to the wrong item. |
| What does `useDebouncedValue`'s cleanup function do? | Cancels the *previous* timer whenever the input value changes again before it fires — that's the entire debounce mechanism. |
| Rules of Hooks? | Only call hooks at a component's top level, never conditionally — React relies on call order to track state across renders. |
| React Router vs. Angular route guards? | Angular: a guard function the router asks "can activate?" React: no such concept — you render a redirect component instead. |
