import { createFeatureSelector, createSelector } from '@ngrx/store';
import * as fromInsightHubStore from './insight-hub-store.reducer';

export const selectInsightHubStoreState = createFeatureSelector<fromInsightHubStore.State>(
  fromInsightHubStore.insightHubStoreFeatureKey
);
