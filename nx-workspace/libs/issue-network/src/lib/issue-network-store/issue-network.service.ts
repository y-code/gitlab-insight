import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { IssueNetwork, IssueNetworkSearchOptions } from "./issue-network.model";
import { InsightHubClientService } from '@youtrack-insight/insight-hub-client';
import { IssueImportTask } from "./issue-network-store.reducer";

@Injectable({
  providedIn: 'root'
})
export class IssueNetworkService {

  get hubClient() { return this.hubClientService; }

  constructor(
    private httpClient: HttpClient,
    private hubClientService: InsightHubClientService,
  ) { }

  getIssues$(options: IssueNetworkSearchOptions): Observable<IssueNetwork> {
    var query = [
      ...options.project.map(x => `project=${encodeURI(x)}`)
    ]
      .reduce((p, c) => `${p}&${c}`, '');
    query = (query ? '?' : '') + query;
    return this.httpClient.get<IssueNetwork>(`/api/YouTrack/issue-network${query}`);
  }

  getIssueImportTasks$(): Observable<IssueImportTask[]> {
    return this.httpClient.get<IssueImportTask[]>('/api/YouTrack/issue-import');
  }

  startIssueImport$(id: string): Observable<void> {
    return this.httpClient.put<void>('/api/YouTrack/issue-import', { id });
  }

  cancelIssueImport$(id: string): Observable<void> {
    return this.httpClient.delete<void>('/api/YouTrack/issue-import', { body: { id } });
  }
}
