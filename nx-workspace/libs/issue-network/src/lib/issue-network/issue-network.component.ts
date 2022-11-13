import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { createSelector, Store } from '@ngrx/store';
import { merge, Subscription, tap } from 'rxjs';
import { cancelIssueImport, connectToInsightHub, disconnectFromInsightHub, getIssueImportTasks, loadIssueNetwork, startIssueImport } from '../issue-network-store/issue-network-store.actions';
import { selectIssueNetworkStoreState } from '../issue-network-store/issue-network-store.selectors';
import { IssueNetworkSearchOptions } from '../issue-network-store/issue-network.model';
import { v1 as generateUuid } from 'uuid';
import { IssueImportTask } from '../issue-network-store/issue-network-store.reducer';

@Component({
  selector: 'youtrack-insight-issue-network',
  templateUrl: './issue-network.component.html',
  styleUrls: ['./issue-network.component.scss'],
})
export class IssueNetworkComponent implements OnInit, OnDestroy {

  options: IssueNetworkSearchOptions = { project: [] };
  conditions: Array<{field: string, value: string}> = [];
  isHubConnected = false;
  hubConnectionState = HubConnectionState.Disconnected;
  isLoadingImportTasks = false;
  importTasks: IssueImportTask[] = [];

  private _subscription?: Subscription;

  @ViewChild("projectConditionDialog")
  projectConditionDialog?: ElementRef;

  @ViewChild("issueImportDialog")
  issueImportDialog?: ElementRef;

  @ViewChild("issueImportTasksDialog")
  issueImportTasksDialog?: ElementRef;

  constructor(
    private store: Store,
    private modalService: NgbModal,
  ) {}

  ngOnInit(): void {
    this._subscription = merge(
      this.store.select(createSelector(selectIssueNetworkStoreState, x => x.issueNetwork)).pipe(
        tap(issueNetwork => {
          this.options = issueNetwork.data?.options ?? { project: [] };

          this.conditions = [
            ...(this.options.project ?? []).map(x => ({ field: 'project', value: x })),
          ];
        }),
      ),

      this.store.select(createSelector(selectIssueNetworkStoreState, x => x.hub)).pipe(
        tap(hub => {
          this.hubConnectionState = hub.state;
          switch (hub.state) {
            case HubConnectionState.Connected:
              this.isHubConnected = true;
              break;
            case HubConnectionState.Disconnected:
              this.isHubConnected = false;
              break;
          }
        }),
      ),

      this.store.select(createSelector(selectIssueNetworkStoreState, x => x.issueImport)).pipe(
        tap(issueImport => {
          this.isLoadingImportTasks = !!issueImport.isLoading;
          if (!this.isLoadingImportTasks)
            this.importTasks = issueImport.tasks;
        }),
      )
    ).subscribe();

    this.store.dispatch(loadIssueNetwork({ options: { project: [] } }));
  }

  ngOnDestroy(): void {
      this._subscription?.unsubscribe();
  }

  async onAddProjectCondition(): Promise<void> {
		const value: string = await this.modalService.open(
      this.projectConditionDialog,
      { ariaLabelledBy: 'modal-basic-title' }
    ).result;
    this.store.dispatch(loadIssueNetwork({
      options: {
        ...this.options,
        project: [
          ...this.options.project,
          value
        ],
      }
    }));
  }

  onRemoveProjectCondition(value: string) {
    this.store.dispatch(loadIssueNetwork({
      options: {
        ...this.options,
        project: this.options.project.filter(x => x !== value),
      }
    }));
  }

  async onIssueImport(): Promise<void> {
    const value: boolean = await this.modalService.open(
      this.issueImportDialog,
      { ariaLabelledBy: 'modal-basic-title' }
    ).result;
    if (value)
      this.store.dispatch(startIssueImport({ importId: generateUuid() }));
  }

  onIssueImportTasks(): void {
    this.store.dispatch(getIssueImportTasks());
    this.modalService.open(
      this.issueImportTasksDialog,
      {
        ariaLabelledBy: 'modal-basic-title',
        size: 'xl',
        fullscreen: 'md',
      }
    );
  }

  onCancelIssueImportTask(importId: string) {
    this.store.dispatch(cancelIssueImport({ importId }));
  }

  onToggleHubConnection() {
    if (this.isHubConnected)
      this.store.dispatch(disconnectFromInsightHub());
    else
      this.store.dispatch(connectToInsightHub());
  }

}
