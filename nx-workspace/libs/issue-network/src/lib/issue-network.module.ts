import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IssueNetworkComponent } from './issue-network/issue-network.component';
import { IssueNetworkMapComponent } from './issue-network-map/issue-network-map.component';
import { IssueNetworkStoreModule } from './issue-network-store/issue-network-store.module';
import { NgbDropdownModule, NgbModalModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  imports: [
    CommonModule,
    IssueNetworkStoreModule,
    NgbDropdownModule,
    NgbModalModule,
  ],
  declarations: [
    IssueNetworkComponent,
    IssueNetworkMapComponent,
  ],
  exports: [
    IssueNetworkComponent,
  ]
})
export class IssueNetworkModule {}
