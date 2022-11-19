import { createFeatureSelector, createSelector } from '@ngrx/store';
import * as fromNotificationStore from './notification-store.reducer';

export const selectNotificationStoreState = createFeatureSelector<fromNotificationStore.State>(
  fromNotificationStore.notificationStoreFeatureKey
);
