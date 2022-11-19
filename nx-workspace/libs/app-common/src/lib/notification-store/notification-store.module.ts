import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreModule } from '@ngrx/store';
import * as fromNotificationStore from './notification-store.reducer';
import { EffectsModule } from '@ngrx/effects';
import { NotificationStoreEffects } from './notification-store.effects';



@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    StoreModule.forFeature(fromNotificationStore.notificationStoreFeatureKey, fromNotificationStore.reducer),
    EffectsModule.forFeature([NotificationStoreEffects])
  ],
  exports: [
    StoreModule,
    EffectsModule,
  ]
})
export class NotificationStoreModule { }
