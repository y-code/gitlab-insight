import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { InsightHubStoreEffects } from './insight-hub-store.effects';

describe('InsightHubStoreEffects', () => {
  let actions$: Observable<any>;
  let effects: InsightHubStoreEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        InsightHubStoreEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.inject(InsightHubStoreEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
