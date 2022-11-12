import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IssueNetworkMapComponent } from '../issue-network-map/issue-network-map.component';
import { initialState } from '../issue-network-store/issue-network-store.reducer';
import { provideMockIssueNetworkStore } from '../issue-network-store/issue-network-store.reducer.spec';

import { IssueNetworkComponent } from './issue-network.component';

describe('IssueNetworkComponent', () => {
  let component: IssueNetworkComponent;
  let fixture: ComponentFixture<IssueNetworkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [IssueNetworkComponent, IssueNetworkMapComponent],
      providers: [provideMockIssueNetworkStore({ initialState })]
    }).compileComponents();

    fixture = TestBed.createComponent(IssueNetworkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
