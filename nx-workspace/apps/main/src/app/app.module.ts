import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NxWelcomeComponent } from './nx-welcome.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { IssueNetworkComponent } from './issue-network/issue-network.component';
import { NavComponent } from './nav/nav.component';
import { IssueNetworkStoreModule } from '../issue-network-store/issue-network-store.module';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';

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
    IssueNetworkStoreModule,
    StoreModule.forRoot({}),
    EffectsModule.forRoot(),
    StoreDevtoolsModule.instrument({ name: 'Node View' }),
  ],
  declarations: [
    AppComponent,
    NxWelcomeComponent,
    IssueNetworkComponent,
    NavComponent,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
