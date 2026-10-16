import { useEffect, useState } from 'react';

/// CUSTOM HOOK: this is the "write a debounce hook" interview exercise, solved with the two
/// hooks everyone reaches for -- useState to hold the delayed value, useEffect to delay setting it.
/// The cleanup function returned from useEffect is the key detail: it cancels the PREVIOUS timer
/// whenever `value` changes again before it fires, which is exactly what debouncing means. Without
/// that `return () => clearTimeout(timer)`, every keystroke would schedule a timer that still fires,
/// and you'd get the LAST n values applied in sequence instead of just the final one.
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timer); // cleanup runs before the NEXT effect, or on unmount
  }, [value, delayMs]);

  return debounced;
}
