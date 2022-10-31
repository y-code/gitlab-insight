import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Store } from '@ngrx/store';
import { tap } from 'rxjs';
import { loadIssueNetworkStores } from '../issue-network-store/issue-network-store.actions';
import { selectIssueNetworkStoreState } from '../issue-network-store/issue-network-store.selectors';
import { IssueNetworkSearchOptions } from '../issue-network-store/issue-network.model';

@Component({
  selector: 'node-view-issue-network',
  templateUrl: './issue-network.component.html',
  styleUrls: ['./issue-network.component.scss'],
})
export class IssueNetworkComponent implements OnInit {

  search: IssueNetworkSearchOptions = { project: [] };
  conditions: Array<{field: string, value: string}> = [];

  @ViewChild("textConditionDialog")
  textConditionDialog?: ElementRef;

  constructor(
    private store: Store,
    private modalService: NgbModal,
  ) {}

  ngOnInit(): void {
    this.store.select(selectIssueNetworkStoreState).pipe(
      tap(state => {
        this.search = state.network?.data?.search ?? { project: [] };

        this.conditions = [
          ...(this.search.project ?? []).map(x => ({ field: 'project', value: x })),
        ];
      }),
    ).subscribe();

    this.store.dispatch(loadIssueNetworkStores({ search: { project: [] } }));
  }

  async onAddProjectCondition(): Promise<void> {
		const value: string = await this.modalService.open(this.textConditionDialog, { ariaLabelledBy: 'modal-basic-title' }).result;
    this.store.dispatch(loadIssueNetworkStores({
      search: {
        ...this.search,
        project: [
          ...this.search.project,
          value
        ],
      }
    }));
  }

  async onRemoveProjectCondition(value: string) {
    this.store.dispatch(loadIssueNetworkStores({
      search: {
        ...this.search,
        project: this.search.project.filter(x => x !== value),
      }
    }));
  }
}
