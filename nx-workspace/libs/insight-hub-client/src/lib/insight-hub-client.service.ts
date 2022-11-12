import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { concat, from, Observable, of, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InsightHubClientService {

  private static hubUrl = 'hub';

  private readonly connection: HubConnection;

  get state() { return this.connection.state; }

  private reconnectionState = new Subject<HubConnectionState>();
  get reconnectionState$() { return this.reconnectionState.asObservable(); }

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl(InsightHubClientService.hubUrl)
      .configureLogging(LogLevel.Debug)
      .withAutomaticReconnect()
      .build();

    this.connection.onreconnecting(err => {
      if (err)
        this.reconnectionState.error(err);
      else
        this.reconnectionState.next(this.connection.state);
    });

    this.connection.onreconnected(err => {
      if (err)
        this.reconnectionState.error(err);
      else
        this.reconnectionState.next(this.connection.state);
    })
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

}
