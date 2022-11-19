import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastsComponent } from './toasts/toasts.component';
import { ToastsModule } from './toasts/toasts.module';
import { NotificationStoreModule } from './notification-store/notification-store.module';

@NgModule({
  imports: [
    ToastsModule,
    NotificationStoreModule,
  ],
  exports: [
    ToastsModule,
    NotificationStoreModule,
  ]
})
export class AppCommonModule {}
