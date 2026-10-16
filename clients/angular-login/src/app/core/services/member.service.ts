import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from '../models/member.model';

@Injectable({ providedIn: 'root' })
export class MemberService {
  constructor(private readonly http: HttpClient) {}

  getMembers(search?: string): Observable<Member[]> {
    const params = search ? { search } : undefined;
    return this.http.get<Member[]>('/api/members', { params });
  }
}
