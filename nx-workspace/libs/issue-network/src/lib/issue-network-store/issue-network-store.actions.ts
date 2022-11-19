import { HubConnectionState } from '@microsoft/signalr';
import { createAction, props } from '@ngrx/store';
import { IssueImportTask } from './issue-network-store.reducer';
import { IssueNetwork, IssueNetworkSearchOptions } from './issue-network.model';

export const loadIssueNetwork = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores',
  props<{ options: IssueNetworkSearchOptions }>()
);

export const loadIssueNetworkSuccess = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Success',
  props<{ data: IssueNetwork }>()
);

export const loadIssueNetworkFailure = createAction(
  '[IssueNetworkStore] Load IssueNetworkStores Failure',
  props<{ error: any }>()
);

export const startIssueImport = createAction(
  '[IssueNetworkStore] Start Issue Import',
  props<{ importId: string }>()
)

export const startIssueImportSuccess = createAction(
  '[IssueNetworkStore] Start Issue Import Success',
  props<{ importId: string }>()
)

export const startIssueImportFailure = createAction(
  '[IssueNetworkStore] Start Issue Import Failure',
  props<{ error: any }>()
)

export const cancelIssueImport = createAction(
  '[IssueNetworkStore] Cancel Issue Import',
  props<{ importId: string }>()
)

export const cancelIssueImportSuccess = createAction(
  '[IssueNetworkStore] Cancel Issue Import Success',
  props<{ importId: string }>()
)

export const cancelIssueImportFailure = createAction(
  '[IssueNetworkStore] Cancel Issue Import Failure',
  props<{ error: any }>()
)

export const getIssueImportTasks = createAction(
  '[IssueNetworkStore] Get Issue Import Tasks'
)

export const getIssueImportTasksSuccess = createAction(
  '[IssueNetworkStore] Get Issue Import Tasks Success',
  props<{ tasks: IssueImportTask[] }>()
)

export const getIssueImportTasksFailure = createAction(
  '[IssueNetworkStore] Get Issue Import Tasks Failure',
  props<{ error: any }>()
)
