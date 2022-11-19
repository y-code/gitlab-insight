import { NgModule } from "@angular/core";
import { EffectsModule } from "@ngrx/effects";
import { StoreModule } from "@ngrx/store";
import { InsightHubStoreEffects } from "./insight-hub-store.effects";
import * as fromInsightHubStore from './insight-hub-store.reducer';

@NgModule({
  imports: [
    StoreModule.forFeature(fromInsightHubStore.insightHubStoreFeatureKey, fromInsightHubStore.reducer),
    EffectsModule.forFeature([InsightHubStoreEffects]),
  ],
  exports: [
    StoreModule,
    EffectsModule,
  ],
})
export class InsightHubStoreModule { }
