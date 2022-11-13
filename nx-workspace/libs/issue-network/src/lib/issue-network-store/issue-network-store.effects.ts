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
        this.service.getIssues$(data.options).pipe(
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

  getIssueImportTasks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.getIssueImportTasks),
      switchMap(() => this.service.getIssueImportTasks$()),
      map(tasks => _Actions.getIssueImportTasksSuccess({ tasks })),
      catchError(error => of(_Actions.getIssueImportTasksFailure({ error }))),
    )
  );

  startIssueImport$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.startIssueImport),
      switchMap(({importId}) =>
        this.service.startIssueImport$(importId).pipe(
          map(() => _Actions.startIssueImportSuccess({ importId })),
          catchError(error => of(_Actions.startIssueImportFailure({ error }))),
        )
      ),
    )
  );

  startIssueImportSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.startIssueImportSuccess),
      map(() => _Actions.getIssueImportTasks()),
    )
  );

  startIssueImportFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.startIssueImportFailure),
      map(() => _Actions.getIssueImportTasks()),
    )
  );

  cancelIssueImport$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.cancelIssueImport),
      switchMap(({importId}) =>
        this.service.cancelIssueImport$(importId).pipe(
          map(() => _Actions.cancelIssueImportSuccess({ importId })),
          catchError(error => of(_Actions.cancelIssueImportFailure({ error }))),
        )
      ),
    )
  );

  cancelIssueImportSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.cancelIssueImportSuccess),
      map(() => _Actions.getIssueImportTasks()),
    )
  );

  cancelIssueImportFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(_Actions.cancelIssueImportFailure),
      map(() => _Actions.getIssueImportTasks()),
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
