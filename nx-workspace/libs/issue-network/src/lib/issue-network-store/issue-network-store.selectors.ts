import { createFeatureSelector, createSelector } from '@ngrx/store';
import * as fromIssueNetworkStore from './issue-network-store.reducer';

export const selectIssueNetworkStoreState = createFeatureSelector<fromIssueNetworkStore.IssueNetworkStoreState>(
  fromIssueNetworkStore.issueNetworkStoreFeatureKey
);
