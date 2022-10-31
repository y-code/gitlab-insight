import { Action, combineReducers, createReducer, on } from '@ngrx/store';
import * as IssueNetworkStoreActions from './issue-network-store.actions';
import { IssueNetwork } from './issue-network.model';

export const issueNetworkStoreFeatureKey = 'issueNetworkStore';

export interface IssuesState {
  isLoading: boolean,
  data?: IssueNetwork,
  error?: any,
}

export interface State {
  network: IssuesState,
}

export const initialState: State = {
  network: {
    isLoading: false,
  }
};

export const reducer = combineReducers({

  network: createReducer(

    initialState.network,

    on(IssueNetworkStoreActions.loadIssueNetworkStores, state => ({
      isLoading: true,
    })),
    on(IssueNetworkStoreActions.loadIssueNetworkStoresSuccess, (state, action) => ({
      ...state,
      isLoading: false,
      data: action.data,
    })),
    on(IssueNetworkStoreActions.loadIssueNetworkStoresFailure, (state, action) => ({
      ...state,
      isLoading: false,
      error: action.error,
    })),

  ),

});
