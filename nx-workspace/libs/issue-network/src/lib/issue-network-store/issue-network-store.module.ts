import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { EffectsModule } from '@ngrx/effects';
import { IssueNetworkStoreEffects } from './issue-network-store.effects';
import { StoreModule } from '@ngrx/store';
import * as fromIssueNetworkStore from './issue-network-store.reducer';

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule,
    StoreModule.forFeature(fromIssueNetworkStore.issueNetworkStoreFeatureKey, fromIssueNetworkStore.reducer),
    EffectsModule.forFeature([IssueNetworkStoreEffects]),
  ],
  declarations: [],
  exports: [
    StoreModule,
    EffectsModule,
  ],
})
export class IssueNetworkStoreModule { }
