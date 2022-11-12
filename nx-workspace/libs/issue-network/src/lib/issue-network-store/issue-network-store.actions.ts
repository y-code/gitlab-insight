import { HubConnectionState } from '@microsoft/signalr';
import { createAction, props } from '@ngrx/store';
import { IssueNetwork, IssueNetworkSearchOptions } from './issue-network.model';

export const loadIssueNetwork = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores',
  props<{ search: IssueNetworkSearchOptions }>()
);

export const loadIssueNetworkSuccess = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Success',
  props<{ data: IssueNetwork }>()
);

export const loadIssueNetworkFailure = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Failure',
  props<{ error: any }>()
);

export const connectToInsightHub = createAction(
  '[IssueNetworkStore] Connect to Insight Hub'
);

export const disconnectFromInsightHub = createAction(
  '[IssueNetworkStore] Disconnect from Insight Hub'
);

export const reflectInsightHubState = createAction(
  '[IssueNetworkStore] Reflect Insight Hub state',
  props<{ state: HubConnectionState }>()
);

export const showMessage = createAction(
  '[IssueNetworkStore] Show message',
  props<{ text: string, isError?: boolean }>()
);
