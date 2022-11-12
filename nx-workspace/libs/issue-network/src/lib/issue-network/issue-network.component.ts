import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Store } from '@ngrx/store';
import { tap } from 'rxjs';
import { connectToInsightHub, disconnectFromInsightHub, loadIssueNetwork } from '../issue-network-store/issue-network-store.actions';
import { selectIssueNetworkStoreState } from '../issue-network-store/issue-network-store.selectors';
import { IssueNetworkSearchOptions } from '../issue-network-store/issue-network.model';

@Component({
  selector: 'gitlab-insight-issue-network',
  templateUrl: './issue-network.component.html',
  styleUrls: ['./issue-network.component.scss'],
})
export class IssueNetworkComponent implements OnInit {

  search: IssueNetworkSearchOptions = { project: [] };
  conditions: Array<{field: string, value: string}> = [];
  isHubConnected = false;
  hubConnectionState = HubConnectionState.Disconnected;

  @ViewChild("projectConditionDialog")
  projectConditionDialog?: ElementRef;

  @ViewChild("issueImportDialog")
  issueImportDialog?: ElementRef;

  constructor(
    private store: Store,
    private modalService: NgbModal,
  ) {}

  ngOnInit(): void {
    this.store.select(selectIssueNetworkStoreState).pipe(
      tap(state => {
        this.search = state.issueNetwork?.data?.search ?? { project: [] };

        this.conditions = [
          ...(this.search.project ?? []).map(x => ({ field: 'project', value: x })),
        ];

        this.hubConnectionState = state.hub.state;
        switch (state.hub.state) {
          case HubConnectionState.Connected:
            this.isHubConnected = true;
            break;
          case HubConnectionState.Disconnected:
            this.isHubConnected = false;
            break;
        }
      }),
    ).subscribe();

    this.store.dispatch(loadIssueNetwork({ search: { project: [] } }));
  }

  async onAddProjectCondition(): Promise<void> {
		const value: string = await this.modalService.open(
      this.projectConditionDialog,
      { ariaLabelledBy: 'modal-basic-title' }
    ).result;
    this.store.dispatch(loadIssueNetwork({
      search: {
        ...this.search,
        project: [
          ...this.search.project,
          value
        ],
      }
    }));
  }

  onRemoveProjectCondition(value: string) {
    this.store.dispatch(loadIssueNetwork({
      search: {
        ...this.search,
        project: this.search.project.filter(x => x !== value),
      }
    }));
  }

  async onIssueImport(): Promise<void> {
    const value: boolean = await this.modalService.open(
      this.issueImportDialog,
      { ariaLabelledBy: 'modal-basic-title' }
    ).result;
    // this.store.dispatch()
  }

  onToggleHubConnection() {
    if (this.isHubConnected)
      this.store.dispatch(disconnectFromInsightHub());
    else
      this.store.dispatch(connectToInsightHub());
  }

}
