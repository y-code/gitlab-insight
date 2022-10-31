import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { IssueModel, IssueNetwork, IssueNetworkSearchOptions } from "./issue-network.model";

@Injectable({
  providedIn: 'root'
})
export class IssueNetworkService {

  constructor(
    private httpClient: HttpClient,
  ) { }

  getIssues$(options: IssueNetworkSearchOptions): Observable<IssueNetwork> {
    var query = [
      ...options.project.map(x => `project=${encodeURI(x)}`)
    ]
      .reduce((p, c) => `${p}&${c}`, '');
    query = (query ? '?' : '') + query;
    return this.httpClient.get<IssueNetwork>(`/api/YouTrack/issue-network${query}`);
  }
}
