import { NgModule } from "@angular/core";
import { InsightHubStoreModule } from "./insight-hub-store/insight-hub-store.module";

@NgModule({
  imports: [
    InsightHubStoreModule,
  ],
  exports: [
    InsightHubStoreModule,
  ],
})
export class InsightHubClientModule { }
