import { Component } from '@angular/core';
import { ToastService } from '@gitlab-insight/app-common';
import { connectToInsightHub, selectIssueNetworkStoreState } from '@gitlab-insight/issue-network';
import { createSelector, Store } from '@ngrx/store';
import { filter, map } from 'rxjs';

@Component({
  selector: 'gitlab-insight-root',
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
          header: 'Insight Hub',
          message: `${state.text||''} to Insight Hub`,
          isError: state.isError,
        })
      }),
    ).subscribe();

    this.store.dispatch(connectToInsightHub());
  }
}
