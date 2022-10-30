import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueNetworkComponent } from './issue-network.component';

describe('HomeComponent', () => {
  let component: IssueNetworkComponent;
  let fixture: ComponentFixture<IssueNetworkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [IssueNetworkComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IssueNetworkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
