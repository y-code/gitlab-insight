import { TestBed } from '@angular/core/testing';
import { AppCommonModule } from '@gitlab-insight/app-common';
import { AppComponent } from './app.component';
import { NxWelcomeComponent } from './nx-welcome.component';
import { initialState } from '../../../../libs/issue-network/src/lib/issue-network-store/issue-network-store.reducer';
import { provideMockIssueNetworkStore } from '../../../../libs/issue-network/src/lib/issue-network-store/issue-network-store.reducer.spec';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppCommonModule],
      declarations: [AppComponent, NxWelcomeComponent],
      providers: [provideMockIssueNetworkStore({ initialState })]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have as title 'main'`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('main');
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('footer')?.textContent).toContain('Y-code');
  });
});
