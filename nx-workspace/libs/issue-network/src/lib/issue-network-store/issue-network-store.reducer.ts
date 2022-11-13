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

export interface IssueImportTask {
  id: string,
  start?: Date,
  end?: Date,
  isCancelled: boolean,
  hasError: boolean,
  message?: string,
}

export interface IssueImportState {
  isLoading?: boolean,
  tasks: IssueImportTask[],
  isSubmitting?: boolean,
  submittingId?: string,
  isCancelling?: boolean,
  cancellingId?: string,
  error?: unknown,
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
  issueImport: IssueImportState,
}

export const initialState: IssueNetworkStoreState = {
  issueNetwork: {
    isLoading: false,
  },
  hub: {
    state: HubConnectionState.Disconnected,
  },
  message: {},
  issueImport: {
    tasks: [],
  },
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

  issueImport: createReducer(

    initialState.issueImport,

    on(_Actions.startIssueImport, (state, action) => ({
      ...state,
      submittingId: action.importId,
      isSubmitting: true,
    })),

    on(_Actions.startIssueImportSuccess, (state, action) => action.importId === state.submittingId
      ? {
        ...state,
        isSubmitting: false,
      }
      : {
        ...state,
      }
    ),

    on(_Actions.startIssueImportFailure, (state, action) => ({
      ...state,
      isSubmitting: false,
      error: action.error,
    })),

    on(_Actions.cancelIssueImport, (state, action) => ({
      ...state,
      cancellingId: action.importId,
      isCancelling: true,
    })),

    on(_Actions.cancelIssueImportSuccess, (state, action) => action.importId === state.cancellingId
      ? {
        ...state,
        isCancelling: false,
      }
      : {
        ...state,
      }
    ),

    on(_Actions.cancelIssueImportFailure, (state, action) => ({
      ...state,
      isSubmitting: false,
      error: action.error,
    })),

    on(_Actions.getIssueImportTasks, (state, action) => ({
      ...state,
      isLoading: true,
    })),

    on(_Actions.getIssueImportTasksSuccess, (state, action) => ({
      ...state,
      isLoading: false,
      tasks: action.tasks,
    })),

    on(_Actions.getIssueImportTasksFailure, (state, action) => ({
      ...state,
      isSubmitting: false,
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