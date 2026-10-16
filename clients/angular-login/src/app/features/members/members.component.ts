import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { MemberService } from '../../core/services/member.service';
import { Member } from '../../core/models/member.model';

/// The route this page lives behind is protected by `authGuard` (app.routes.ts) -- reaching this
/// component at all already proves a valid token exists. The search box below demonstrates the
/// classic RxJS "search-as-you-type" combinator chain, independent of the auth story.
@Component({
  selector: 'app-members',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './members.component.html',
  styleUrl: './members.component.scss',
})
export class MembersComponent {
  private readonly destroyRef = inject(DestroyRef);

  readonly members = signal<Member[]>([]);
  readonly searchControl = new FormControl('', { nonNullable: true });

  constructor(
    private readonly memberService: MemberService,
    readonly auth: AuthService,
    private readonly router: Router
  ) {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300), // wait for the user to stop typing
        distinctUntilChanged(), // skip re-querying if the value didn't actually change
        // switchMap: if a new keystroke arrives before the previous HTTP call resolves, switchMap
        // CANCELS that stale inner Observable and switches to the new one -- exactly what you want
        // for type-ahead search (an old slow response can never clobber a newer one). Contrast with
        // mergeMap (runs all requests concurrently, any order back) and concatMap (queues them,
        // strict order, no cancellation) -- a direct answer if asked to compare the three.
        switchMap((term) => this.memberService.getMembers(term || undefined)),
        // takeUntilDestroyed replaces manual ngOnDestroy + Subject<void> + takeUntil boilerplate --
        // the subscription is torn down automatically when this component is destroyed.
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((members) => this.members.set(members));

    this.memberService
      .getMembers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((members) => this.members.set(members));
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
