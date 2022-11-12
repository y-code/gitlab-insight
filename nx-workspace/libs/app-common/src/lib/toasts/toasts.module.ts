import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { NgbModule, NgbToastModule } from "@ng-bootstrap/ng-bootstrap";
import { ToastsComponent } from "./toasts.component";

@NgModule({
  imports: [
    CommonModule,
    NgbModule,
    NgbToastModule,
  ],
  declarations: [
    ToastsComponent,
  ],
  exports: [
    ToastsComponent,
  ]
})
export class ToastsModule {}
