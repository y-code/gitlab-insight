import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { createSelector, Store } from '@ngrx/store';
import { map, merge, Observable, Subscription, tap, timer } from 'rxjs';
import { selectInsightHubStoreState, subscribeToIssueImport } from '@youtrack-insight/insight-hub-client';
import { cancelIssueImport, getIssueImportTasks, loadIssueNetwork, startIssueImport } from '../issue-network-store/issue-network-store.actions';
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
  importTasks: (IssueImportTask&{
    statusIcon: {[key: string]:boolean},
    formattedDuration: Observable<string|undefined>,
  })[] = [];

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

      this.store.select(createSelector(selectInsightHubStoreState, x => x.connection)).pipe(
        tap(connection => {
          this.hubConnectionState = connection.state;
          switch (connection.state) {
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
            this.importTasks = issueImport.tasks.map(x => ({
              ...x,
              statusIcon: this.getImportTaskStateIcon(x),
              formattedDuration: timer(0, 1000).pipe(
                map(() => this.formatImportTaskDuration(x)),
              ),
            }));
        }),
      )
    ).subscribe();

    this.store.dispatch(subscribeToIssueImport());
    this.store.dispatch(loadIssueNetwork({ options: { project: [] } }));
  }

  ngOnDestroy(): void {
      this._subscription?.unsubscribe();
  }

  private formatImportTaskDuration(task: IssueImportTask): string|undefined {
    const duration = task.start
      ? (task.end?.getTime() ?? Date.now()) - task.start.getTime()
      : undefined;
    if (duration === undefined)
      return undefined;
    const hours = Math.floor(duration / 1000 / 60 / 60);
    const minutes = ('' + Math.floor(duration / 1000 / 60)).padStart(2, '0');
    const seconds = ('' + Math.floor(duration / 1000)).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
  }

  private getImportTaskStateIcon(task: IssueImportTask): {[key: string]: boolean} {
    return {
      'bi-emoji-smile': !!task.end && !task.isCancelled && !task.hasError,
      'bi-clock': !!task.start && !task.end && !task.isCancelled && !task.hasError,
      'bi-emoji-dizzy': task.hasError,
      'bi-emoji-expressionless': task.isCancelled && !task.hasError,
    }
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
}
