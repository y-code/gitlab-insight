import { createAction, props } from '@ngrx/store';

export const showNotification = createAction(
  '[IssueNetworkStore] Show notification',
  props<{ content: any, isError?: boolean }>()
);
