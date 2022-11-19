import { Injectable } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { createSelector, Store } from '@ngrx/store';
import { showNotification } from '@youtrack-insight/app-common';

import { catchError, concatMap, filter, first, map, merge, mergeMap, of, switchMap } from 'rxjs';
import { InsightHubClientService } from '../insight-hub-client.service';
import * as _Actions from './insight-hub-store.actions';
import { selectInsightHubStoreState } from './insight-hub-store.selectors';

@Injectable()
export class InsightHubStoreEffects {

  connectToInsightHub$ = createEffect(() => this.actions$.pipe( 
    ofType(_Actions.connectToHub),
    concatMap(() => this.service.connect$()),
    mergeMap(() => merge([
      _Actions.connectToHubSuccess({ state: this.service.state }),
      showNotification({ content: `${this.service.state} to Insight Hub` })
    ])),
    catchError(error => of(_Actions.connectToHubFailure({ error }))),
  ));

  disconnectToInsightHub$ = createEffect(() => this.actions$.pipe( 
    ofType(_Actions.disconnectFromHub),
    concatMap(() => this.service.disconnect$()),
    mergeMap(() => merge([
      _Actions.disconnectFromHubSuccess({ state: this.service.state }),
      showNotification({ content: `${this.service.state} to Insight Hub` })
    ])),
    catchError(error => of(_Actions.disconnectFromHubFailure({ error }))),
  ));

  connectToHubFailure$ = createEffect(() => this.actions$.pipe(
    ofType(_Actions.connectToHubFailure),
    map(({error}) => showNotification({ content: error, isError: true })),
  ));

  disconnectFromHubFailure$ = createEffect(() => this.actions$.pipe(
    ofType(_Actions.disconnectFromHubFailure),
    map(({error}) => showNotification({ content: error, isError: true })),
  ));

  subscribeToIssueImport$ = createEffect(() => this.actions$.pipe( 
    ofType(_Actions.subscribeToIssueImport),
    mergeMap(() => this.service.firstConnected$),
    concatMap(() => this.service.subscribeToIssueImport$()),
    map(() => _Actions.subscribeToIssueImportSuccess()),
    catchError(error => of(_Actions.subscribeToIssueImportFailure({ error }))),
  ));

  subscribeToIssueImportFailure$ = createEffect(() => this.actions$.pipe(
    ofType(_Actions.subscribeToIssueImportFailure),
    map(({error}) => showNotification({ content: error, isError: true })),
  ));

  unsubscribeToIssueImport$ = createEffect(() => this.actions$.pipe( 
    ofType(_Actions.unsubscribeToIssueImport),
    mergeMap(() => this.service.firstConnected$),
    concatMap(() => this.service.unsubscribeToIssueImport$()),
    map(() => _Actions.unsubscribeToIssueImportSuccess()),
    catchError(error => of(_Actions.unsubscribeToIssueImportFailure({ error }))),
  ));

  unsubscribeToIssueImportFailure$ = createEffect(() => this.actions$.pipe(
    ofType(_Actions.unsubscribeToIssueImportFailure),
    map(({error}) => showNotification({ content: error, isError: true })),
  ));

  constructor(
    private actions$: Actions,
    private store: Store,
    private service: InsightHubClientService,
  ) {}
}
