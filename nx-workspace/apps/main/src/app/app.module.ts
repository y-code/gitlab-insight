import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NxWelcomeComponent } from './nx-welcome.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { IssueNetworkComponent, IssueNetworkModule } from '@gitlab-insight/issue-network';
import { NavComponent } from './nav/nav.component';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { AppCommonModule } from '@gitlab-insight/app-common';

@NgModule({
  imports: [
    BrowserModule,
    NgbModule,
    RouterModule.forRoot([
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'issue-network',
      },
      {
        path: 'nx-welcome',
        component: NxWelcomeComponent,
        title: 'Nx Welcome',
      },
      {
        path: 'issue-network',
        component: IssueNetworkComponent,
        title: 'Issue Network',
      },
    ]),
    StoreModule.forRoot({}),
    EffectsModule.forRoot(),
    StoreDevtoolsModule.instrument({ name: 'Node View' }),
    AppCommonModule,
    IssueNetworkModule,
  ],
  declarations: [
    AppComponent,
    NxWelcomeComponent,
    NavComponent,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
