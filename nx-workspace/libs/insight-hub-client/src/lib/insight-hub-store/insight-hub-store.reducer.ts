import { HubConnectionState } from '@microsoft/signalr';
import { Action, combineReducers, createReducer, on } from '@ngrx/store';
import * as _Actions from './insight-hub-store.actions';

export const insightHubStoreFeatureKey = 'insightHubStore';

export interface ConnectionState {
  isOperating: boolean,
  operationStart?: Date,
  state: HubConnectionState,
  error?: any,
}

export interface State {
  connection: ConnectionState,
}

export const initialState: State = {
  connection: {
    isOperating: false,
    state: HubConnectionState.Disconnected,
  },
};

export const reducer = combineReducers({
  connection: createReducer(

    initialState.connection,
  
    on(_Actions.connectToHub, state => ({
      ...state,
      isOperating: true,
      operationStart: new Date(),
      error: undefined,
    })),
    on(_Actions.connectToHubSuccess, (state, action) => ({
      ...state,
      isOperating: false,
      state: action.state,
    })),
    on(_Actions.connectToHubFailure, (state, action) => ({
      ...state,
      isOperating: false,
      error: action.error,
    })),

    on(_Actions.disconnectFromHub, state => ({
      ...state,
      isOperating: true,
      operationStart: new Date(),
      error: undefined,
    })),
    on(_Actions.disconnectFromHubSuccess, (state, action) => ({
      ...state,
      isOperating: false,
      operationStart: new Date(),
      state: action.state,
    })),
    on(_Actions.disconnectFromHubFailure, (state, action) => ({
      ...state,
      isOperating: false,
      operationStart: new Date(),
      error: action.error,
    })),

  ),
});
