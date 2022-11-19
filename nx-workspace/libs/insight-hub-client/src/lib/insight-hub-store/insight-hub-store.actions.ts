import { HubConnectionState } from '@microsoft/signalr';
import { createAction, props } from '@ngrx/store';

export const connectToHub = createAction(
  '[InsightHubStore] Connect to Insight Hub'
);

export const connectToHubSuccess = createAction(
  '[InsightHubStore] Connect to Insight Hub Success',
  props<{ state: HubConnectionState }>()
);

export const connectToHubFailure = createAction(
  '[InsightHubStore] Connect to Insight Hub Failure',
  props<{ error: any }>()
);

export const disconnectFromHub = createAction(
  '[InsightHubStore] Disconnect from Insight Hub'
);

export const disconnectFromHubSuccess = createAction(
  '[InsightHubStore] Disconnect from Insight Hub Success',
  props<{ state: HubConnectionState }>()
);

export const disconnectFromHubFailure = createAction(
  '[InsightHubStore] Disconnect from Insight Hub Failure',
  props<{ error: any }>()
);

export const subscribeToIssueImport = createAction(
  '[InsightHubStore] subscribeToIssueImport call to Insight Hub'
);

export const subscribeToIssueImportSuccess = createAction(
  '[InsightHubStore] subscribeToIssueImport call to Insight Hub Success'
);

export const subscribeToIssueImportFailure = createAction(
  '[InsightHubStore] subscribeToIssueImport call to Insight Hub Failure',
  props<{ error: any }>()
);

export const unsubscribeToIssueImport = createAction(
  '[InsightHubStore] unsubscribeToIssueImport call to Insight Hub'
);

export const unsubscribeToIssueImportSuccess = createAction(
  '[InsightHubStore] unsubscribeToIssueImport call to Insight Hub Success'
);

export const unsubscribeToIssueImportFailure = createAction(
  '[InsightHubStore] unsubscribeToIssueImport call to Insight Hub Failure',
  props<{ error: any }>()
);

export const onIssueImportTaskUpdated = createAction(
  '[InsightHubStore] OnIssueImportTaskUpdated call from Hub',
  props<{ taskId: string }>()
)
