import { Component } from '@angular/core';
import { ToastService } from '@youtrack-insight/app-common';
import { connectToInsightHub, selectIssueNetworkStoreState } from '@youtrack-insight/issue-network';
import { createSelector, Store } from '@ngrx/store';
import { filter, map } from 'rxjs';

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
    this.store.select(createSelector(selectIssueNetworkStoreState, state => state.message)).pipe(
      filter(state => !!state.text),
      map(state => {
        this.toast.add({
          header: 'Issue Network',
          message: state.text||'',
          isError: state.isError,
        })
      }),
    ).subscribe();

    this.store.dispatch(connectToInsightHub());
  }
}
