import { Component } from '@angular/core';
import { selectNotificationStoreState, ToastService } from '@youtrack-insight/app-common';
import { selectIssueNetworkStoreState } from '@youtrack-insight/issue-network';
import { createSelector, Store } from '@ngrx/store';
import { filter, map } from 'rxjs';
import { connectToHub } from '@youtrack-insight/insight-hub-client';

@Component({
  selector: 'youtrack-insight-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'main';

  constructor(
    private store: Store,
    private toast: ToastService,
    ) {
    this.store.select(selectNotificationStoreState).pipe(
      filter(state => !!state.text),
      map(state => {
        this.toast.add({
          header: 'Issue Network',
          message: state.text||'',
          isError: state.isError,
        })
      }),
    ).subscribe();

    this.store.dispatch(connectToHub());
  }
}
