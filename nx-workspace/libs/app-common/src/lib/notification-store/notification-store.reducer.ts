import { Action, createReducer, on } from '@ngrx/store';
import * as _Actions from './notification-store.actions';

export const notificationStoreFeatureKey = 'notificationStore';

export interface State {
  text?: string,
  isError?: boolean,
}

export const initialState: State = {
};

export const reducer = createReducer(

  initialState,

  on(_Actions.showNotification, (state, action) => ({
    text: assembleError(action.content),
    isError: action.isError,
  })),

);

function assembleError(err: any): any {
  let message = '';
  if (!err) {}
  else if (typeof(err) === 'string')
    message += err;
  else {
    if (!err.error) {}
    else if (typeof(err.error) === 'string')
      message += (message?'\n':'') + err.error;
    else {
      if (!err.error.detail) {}
      else if (typeof(err.error.detail) === 'string')
        message += (message?'\n':'') + err.error.detail;
    }
    if (!err.message) {}
    else if (typeof(err.message) === 'string')
      message += (message?'\n':'') + err.message;
    if (!message)
      return err;
  }
  return message;
}
