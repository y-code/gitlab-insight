import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of } from 'rxjs';

import { IssueNetworkStoreEffects } from './issue-network-store.effects';
import { IssueNetworkService } from './issue-network.service';
import { IssueNetwork } from './issue-network.model';
import { provideMockIssueNetworkStore } from './issue-network-store.reducer.spec';
import { initialState } from './issue-network-store.reducer';

describe('IssueNetworkStoreEffects', () => {
  let actions$: Observable<any>;
  let effects: IssueNetworkStoreEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
      providers: [
        IssueNetworkStoreEffects,
        provideMockActions(() => actions$),
        provideMockIssueNetworkStore({ initialState }),
      ]
    });

    const service = TestBed.inject(IssueNetworkService);
    jest.spyOn(service, 'getIssues$').mockReturnValue(of({} as IssueNetwork));
    effects = TestBed.inject(IssueNetworkStoreEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
