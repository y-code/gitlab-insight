import { HubConnectionState } from '@microsoft/signalr';
import { combineReducers, createReducer, on } from '@ngrx/store';
import * as _Actions from './issue-network-store.actions';
import { IssueNetwork } from './issue-network.model';

export const issueNetworkStoreFeatureKey = 'issueNetworkStore';

export interface IssuesState {
  isLoading: boolean,
  data?: IssueNetwork,
  error?: any,
}

export interface HubState {
  state: HubConnectionState,
  operationStart?: Date,
}

export interface MessageState {
  text?: string,
  isError?: boolean,
}

export interface IssueNetworkStoreState {
  issueNetwork: IssuesState,
  hub: HubState,
  message: MessageState,
}

export const initialState: IssueNetworkStoreState = {
  issueNetwork: {
    isLoading: false,
  },
  hub: {
    state: HubConnectionState.Disconnected,
  },
  message: {},
};

export const reducer = combineReducers({

  issueNetwork: createReducer(

    initialState.issueNetwork,

    on(_Actions.loadIssueNetwork, state => ({
      isLoading: true,
    })),
    on(_Actions.loadIssueNetworkSuccess, (state, action) => ({
      ...state,
      isLoading: false,
      data: action.data,
    })),
    on(_Actions.loadIssueNetworkFailure, (state, action) => ({
      ...state,
      isLoading: false,
      error: action.error,
    })),

  ),

  hub: createReducer(

    initialState.hub,

    on(_Actions.connectToInsightHub, state => ({
      ...state,
      operationStart: new Date(),
    })),
    on(_Actions.disconnectFromInsightHub, state => ({
      ...state,
      operationStart: new Date(),
    })),
    on(_Actions.reflectInsightHubState, (state, action) => ({
      state: action.state,
    })),

  ),

  message: createReducer<MessageState|undefined>(

    initialState.message,

    on(_Actions.reflectInsightHubState, (state, action) => ({
      text: action.state,
    })),

    on(_Actions.showMessage, (state, action) => ({
      text: action.text,
      isError: action.isError,
    })),

  ),

});
