import { Store } from '@ngrx/store';
import { getMockStore } from '@ngrx/store/testing';
import { reducer, initialState, IssueNetworkStoreState as IssueNetworkState, issueNetworkStoreFeatureKey } from './issue-network-store.reducer';

export const provideMockIssueNetworkStore =
  (config: { initialState: IssueNetworkState }) =>
    ({
      provide: Store,
      useValue: getMockStore({
        initialState: {
          [issueNetworkStoreFeatureKey]: config.initialState,
        },
      }),
    });

describe('IssueNetworkStore Reducer', () => {
  describe('an unknown action', () => {
    it('should return the previous state', () => {
      const action = {} as any;

      const result = reducer(initialState, action);

      expect(result).toBe(initialState);
    });
  });
});
