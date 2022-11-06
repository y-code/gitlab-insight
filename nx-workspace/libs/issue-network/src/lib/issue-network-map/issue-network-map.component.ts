import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { Subscription, tap } from 'rxjs';
import { Network, Node, Edge, Options } from 'vis-network';
import { DataSet } from 'vis-data';
import { loadIssueNetworkStores } from '../issue-network-store/issue-network-store.actions';
import { selectIssueNetworkStoreState } from '../issue-network-store/issue-network-store.selectors';
import { IssueModel, IssueNetwork, IssueLinkModel } from '../issue-network-store/issue-network.model';

@Component({
  selector: 'gitlab-insight-issue-network-map',
  templateUrl: './issue-network-map.component.html',
  styleUrls: ['./issue-network-map.component.scss'],
})
export class IssueNetworkMapComponent implements OnInit, OnDestroy {

  @ViewChild('issueNetwork') issueNetwork?: ElementRef<HTMLDivElement>;

  subscription?: Subscription;

  data: IssueNetwork = { search: { project: [] }, issues: [], links: [] };

  network?: Network;

  colors: { [id: string]: string } = {};

  constructor(
    private store: Store,
  ) {}

  ngOnInit(): void {
    this.subscription = this.store.select(selectIssueNetworkStoreState).pipe(
      tap(state => {
        this.data = state.network?.data ?? { search: { project: [] }, issues: [], links: [] };

        if (this.issueNetwork?.nativeElement) {
          const nodes = new DataSet<Node>(
            this.data.issues.map(x => {
              return {
                id: x.id,
                label: `[${x.id}] ${x.summary}`,
                color: this.colors[x.topId] ??= this.generateRandomColor(),
                borderWidth: x.id === x.topId ? 6 : 2,
                font: { size: this.getFontSize(x) },
              };
            })
          );
          const edges = new DataSet<Edge>(
            this.data.links.map(y => {
              const source = this.data.issues.find(x => x.id === y.source);
              const target = this.data.issues.find(x => x.id === y.target);
              return {
                from: y.source,
                to: y.target,
                label: y.type,
                width: source ? this.getEdgeWidth(source) : 1,
                dashes: this.isEdgeDashes(y),
              };
            })
          );
          const options: Options = {
            edges: {
              arrows: {
                from: {
                  enabled: true,
                  type: 'arrow',
                },
              },
              length: 200,
            },
          };
          this.network = new Network(this.issueNetwork?.nativeElement, { nodes, edges }, options)
        }
      }),
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private generateRandomColor(): string {
    const max = 256;
    const r = max - Math.floor(Math.random() * max / 2);
    const g = max - Math.floor(Math.random() * max / 2);
    const b = max - Math.floor(Math.random() * max / 2);
    return `rgb(${r},${g},${b})`;
  }

  private getFontSize(issue: IssueModel): number {
    switch (issue.level) {
      case 0: return 20;
      case 1: return 18;
      case 2: return 16;
      case 3: return 14;
      default: return 12;
    }
  }

  private getEdgeWidth(issue: IssueModel): number {
    switch (issue.level) {
      case 0: return 5;
      case 1: return 3;
      default: return 1;
    }
  }

  private isEdgeDashes(link: IssueLinkModel): boolean {
    switch (link.type) {
      case 'Subtask':
      case 'Depend':
        return false;
      default:
        return true;
    }
  }
}
