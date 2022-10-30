import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, concatMap } from 'rxjs/operators';
import { of } from 'rxjs';
import * as IssueNetworkStoreActions from './issue-network-store.actions';
import { IssueNetworkService } from './issue-network.service';

@Injectable()
export class IssueNetworkStoreEffects {

  loadIssueNetworkStores$ = createEffect(() => {
    return this.actions$.pipe( 

      ofType(IssueNetworkStoreActions.loadIssueNetworkStores),
      concatMap(() =>
        this.service.getIssues$().pipe(
          map(data => IssueNetworkStoreActions.loadIssueNetworkStoresSuccess({ data })),
          catchError(error => of(IssueNetworkStoreActions.loadIssueNetworkStoresFailure({ error }))))
      )
    );
  });

  constructor(
    private actions$: Actions,
    private service: IssueNetworkService,
  ) {}
}
