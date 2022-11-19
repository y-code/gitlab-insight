import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';

import { concatMap } from 'rxjs/operators';
import { Observable, EMPTY } from 'rxjs';
import * as _Actions from './notification-store.actions';

@Injectable()
export class NotificationStoreEffects {

  constructor(private actions$: Actions) {}
}
