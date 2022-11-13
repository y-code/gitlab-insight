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

export interface IssueImportState {
  isRequesting?: boolean,
  importId?: string,
  error?: unknown
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
  import: IssueImportState,
}

export const initialState: IssueNetworkStoreState = {
  issueNetwork: {
    isLoading: false,
  },
  hub: {
    state: HubConnectionState.Disconnected,
  },
  message: {},
  import: {},
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

  message: createReducer(

    initialState.message,

    on(_Actions.reflectInsightHubState, (state, action) => ({
      text: `${action.state} to Insight Hub`,
    })),

    on(_Actions.showMessage, (state, action) => ({
      text: assembleError(action.text),
      isError: action.isError,
    })),


    on(_Actions.startIssueImportFailure, (state, action) => ({
      text: assembleError(action.error),
      isError: true,
    })),

  ),

  import: createReducer(

    initialState.import,

    on(_Actions.startIssueImport, (state, action) => ({
      importId: action.importId,
      isRequesting: true,
    })),

    on(_Actions.startIssueImportSuccess, (state, action) => action.importId === state.importId
      ? {
        ...state,
        importId: action.importId,
        isRequesting: false,
      }
      : {
        ...state,
      }
    ),

    on(_Actions.startIssueImportFailure, (state, action) => ({
      ...state,
      isRequesting: false,
      error: action.error,
    })),

  )

});

function assembleError(err: any): any {
  let message = '';
  if (!err) {}
  else if (typeof(err) === 'string')
    message += err;
  else {
    if (!err.message) {}
    else if (typeof(err.message) === 'string')
      message += (message?'\n':'') + err.message;
    if (!err.error) {}
    else if (typeof(err.error) === 'string')
      message += (message?'\n':'') + err.error;
    if (!message)
      return err;
  }
  return message;
}