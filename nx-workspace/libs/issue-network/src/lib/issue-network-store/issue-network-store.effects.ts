import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { catchError, concatMap, map, merge, of, switchMap, tap } from 'rxjs';
import * as _Actions from './issue-network-store.actions';
import { IssueNetworkService } from './issue-network.service';

@Injectable()
export class IssueNetworkStoreEffects {

  loadIssueNetwork$ = createEffect(() =>
    this.actions$.pipe( 
      ofType(_Actions.loadIssueNetwork),
      concatMap(data =>
        this.service.getIssues$(data.search).pipe(
          map(data => _Actions.loadIssueNetworkSuccess({ data })),
          catchError(error => of(_Actions.loadIssueNetworkFailure({ error }))))
      )
    )
  );

  connectToInsightHub$ = createEffect(() =>
    this.actions$.pipe( 
      ofType(_Actions.connectToInsightHub),
      concatMap(() => this.service.hubClient.connect$()),
      map(() => _Actions.reflectInsightHubState({ state: this.service.hubClient.state })),
      catchError(err => of(_Actions.showMessage({ text: err, isError: true }))),
    )
  );

  disconnectToInsightHub$ = createEffect(() =>
    this.actions$.pipe( 
      ofType(_Actions.disconnectFromInsightHub),
      concatMap(() => this.service.hubClient.disconnect$()),
      map(() => _Actions.reflectInsightHubState({ state: this.service.hubClient.state })),
      catchError(err => of(_Actions.showMessage({ text: err, isError: true }))),
    )
  );

  start$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.startIssueImport),
      switchMap(({importId}) =>
        this.service.startIssueImport$(importId).pipe(
          map(() => _Actions.startIssueImportSuccess({ importId: importId })),
        )
      ),
      catchError(err => of(_Actions.startIssueImportFailure({ error: err }))),
    )
  );

  constructor(
    private actions$: Actions,
    private service: IssueNetworkService,
    private store: Store,
  ) {
    this.service.hubClient.reconnectionState$.pipe(
      tap(state => this.store.dispatch(_Actions.reflectInsightHubState({ state }))),
    ).subscribe();
  }
}
