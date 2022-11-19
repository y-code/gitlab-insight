import * as fromNotificationStore from './notification-store.reducer';
import { selectNotificationStoreState } from './notification-store.selectors';

describe('NotificationStore Selectors', () => {
  it('should select the feature state', () => {
    const result = selectNotificationStoreState({
      [fromNotificationStore.notificationStoreFeatureKey]: {}
    });

    expect(result).toEqual({});
  });
});
