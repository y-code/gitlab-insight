import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { IssueNetworkStoreEffects } from './issue-network-store.effects';

describe('IssueNetworkStoreEffects', () => {
  let actions$: Observable<any>;
  let effects: IssueNetworkStoreEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        IssueNetworkStoreEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.inject(IssueNetworkStoreEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
