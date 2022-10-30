import { createAction, props } from '@ngrx/store';
import { IssueNetwork } from './issue-network.model';

export const loadIssueNetworkStores = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores'
);

export const loadIssueNetworkStoresSuccess = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Success',
  props<{ data: IssueNetwork }>()
);

export const loadIssueNetworkStoresFailure = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Failure',
  props<{ error: any }>()
);
