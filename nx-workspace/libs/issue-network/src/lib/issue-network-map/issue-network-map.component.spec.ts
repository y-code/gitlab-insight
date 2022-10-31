import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueNetworkMapComponent } from './issue-network-map.component';

describe('IssueNetworkMapComponent', () => {
  let component: IssueNetworkMapComponent;
  let fixture: ComponentFixture<IssueNetworkMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [IssueNetworkMapComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IssueNetworkMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
