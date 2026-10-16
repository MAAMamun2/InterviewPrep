import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiFetch } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import type { Member } from '../types';

export function MembersPage() {
  const [members, setMembers] = useState<Member[]>([]);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, 300); // React's answer to Angular's debounceTime(300)

  const { username, role, logout } = useAuth();
  const navigate = useNavigate();

  // useEffect with a dependency array: runs once on mount (empty deps would be `[]`) and again
  // whenever `debouncedSearch` changes -- NOT on every `search` keystroke, because debouncedSearch
  // only updates 300ms after typing stops. This is the plain-useEffect equivalent of the Angular
  // page's switchMap chain, just without RxJS's built-in request-cancellation; a real app would
  // track a `cancelled` flag or AbortController here to avoid a late response overwriting a newer one.
  useEffect(() => {
    let cancelled = false;

    async function load() {
      const query = debouncedSearch ? `?search=${encodeURIComponent(debouncedSearch)}` : '';
      const result = await apiFetch<Member[]>(`/api/members${query}`);
      if (!cancelled && result.status === 'success') {
        setMembers(result.data);
      }
    }

    load();
    return () => {
      cancelled = true; // avoids the classic "setState after unmount / stale response" bug
    };
  }, [debouncedSearch]);

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className="members-page">
      <header>
        <h1>Members</h1>
        <div className="who">
          <span>{username} ({role})</span>
          <button onClick={handleLogout}>Log out</button>
        </div>
      </header>

      <input
        type="search"
        placeholder="Search by name…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <table>
        <thead>
          <tr>
            <th>Name</th><th>Age</th><th>Gender</th><th>Tier</th><th>Purchases</th>
          </tr>
        </thead>
        <tbody>
          {members.length === 0 ? (
            <tr><td colSpan={5}>No members found.</td></tr>
          ) : (
            // `key={member.id}` -- React's reconciliation needs a STABLE, UNIQUE key per list item
            // to correctly diff/reorder/preserve state across re-renders. Using the array INDEX as
            // the key instead is the classic antipattern: it breaks the moment items are
            // inserted/removed/reordered, because React then matches the wrong old DOM node to the
            // wrong new item.
            members.map((member) => (
              <tr key={member.id}>
                <td>{member.name}</td>
                <td>{member.age}</td>
                <td>{member.gender}</td>
                <td>{member.tier}</td>
                <td>{member.purchaseCount}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
