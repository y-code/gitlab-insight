import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { NotificationStoreEffects } from './notification-store.effects';

describe('NotificationStoreEffects', () => {
  let actions$: Observable<any>;
  let effects: NotificationStoreEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        NotificationStoreEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.inject(NotificationStoreEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
