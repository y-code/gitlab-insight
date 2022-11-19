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

export interface IssueNetworkStoreState {
  issueNetwork: IssuesState,
  issueImport: IssueImportState,
}

export const initialState: IssueNetworkStoreState = {
  issueNetwork: {
    isLoading: false,
  },
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
