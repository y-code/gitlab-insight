import * as fromInsightHubStore from './insight-hub-store.reducer';
import { selectInsightHubStoreState } from './insight-hub-store.selectors';

describe('InsightHubStore Selectors', () => {
  it('should select the feature state', () => {
    const result = selectInsightHubStoreState({
      [fromInsightHubStore.insightHubStoreFeatureKey]: {}
    });

    expect(result).toEqual({});
  });
});
