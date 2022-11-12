import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastsComponent } from './toasts/toasts.component';
import { ToastsModule } from './toasts/toasts.module';

@NgModule({
  imports: [
    ToastsModule,
  ],
  exports: [
    ToastsModule,
  ]
})
export class AppCommonModule {}
