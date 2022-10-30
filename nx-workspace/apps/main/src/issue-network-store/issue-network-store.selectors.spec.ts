import * as fromIssueNetworkStore from './issue-network-store.reducer';
import { selectIssueNetworkStoreState } from './issue-network-store.selectors';

describe('IssueNetworkStore Selectors', () => {
  it('should select the feature state', () => {
    const result = selectIssueNetworkStoreState({
      [fromIssueNetworkStore.issueNetworkStoreFeatureKey]: {}
    });

    expect(result).toEqual({});
  });
});
