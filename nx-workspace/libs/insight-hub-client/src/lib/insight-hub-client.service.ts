import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { createSelector, Store } from '@ngrx/store';
import { concat, filter, first, from, Observable, of, Subject } from 'rxjs';
import { connectToHubFailure, connectToHubSuccess, onIssueImportTaskUpdated } from './insight-hub-store/insight-hub-store.actions';
import { selectInsightHubStoreState } from './insight-hub-store/insight-hub-store.selectors';

@Injectable({
  providedIn: 'root',
})
export class InsightHubClientService {

  private static hubUrl = 'hub';

  private readonly connection: HubConnection;

  get state() { return this.connection.state; }

  readonly firstConnected$
    = this.store.select(createSelector(selectInsightHubStoreState, x => x.connection))
      .pipe(
        filter(x => x.state === HubConnectionState.Connected),
        first(),
      );

  constructor(
    private store: Store,
  ) {
    this.connection = new HubConnectionBuilder()
      .withUrl(InsightHubClientService.hubUrl)
      .configureLogging(LogLevel.Debug)
      .withAutomaticReconnect()
      .build();

    this.connection.onreconnecting(error => {
      if (error)
        this.store.dispatch(connectToHubFailure({ error }))
      else
        this.store.dispatch(connectToHubSuccess({ state: this.connection.state }));
    });

    this.connection.onreconnected(error => {
      if (error)
        this.store.dispatch(connectToHubFailure({ error }))
      else
        this.store.dispatch(connectToHubSuccess({ state: this.connection.state }));
    })

    this.connection.on('OnIssueImportTaskUpdated', (taskId: string) =>
      this.store.dispatch(onIssueImportTaskUpdated({ taskId })));
  }

  connect$(): Observable<void> {
    switch (this.connection.state) {
      case HubConnectionState.Connected:
      case HubConnectionState.Connecting:
      case HubConnectionState.Reconnecting:
        return of(void 0);
      default:
        return concat(
          of(void 0),
          from(this.connection.start())
        );
    }
  }

  disconnect$(): Observable<void> {
    switch (this.connection.state) {
      case HubConnectionState.Disconnected:
      case HubConnectionState.Disconnecting:
        return of(void 0);
      default:
        return concat(
          of(void 0),
          from(this.connection.stop())
        );
    }
  }

  subscribeToIssueImport$(): Observable<void> {
    return from(this.connection.send('SubscribeToIssueImport'));
  }

  unsubscribeToIssueImport$(): Observable<void> {
    return from(this.connection.send('UnsubscribeToIssueImport'));
  }
}
