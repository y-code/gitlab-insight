import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { IssueModel, IssueNetwork } from "./issue-network.model";

@Injectable({
  providedIn: 'root'
})
export class IssueNetworkService {

  constructor(
    private httpClient: HttpClient,
  ) { }

  getIssues$(): Observable<IssueNetwork> {
    return this.httpClient.get<IssueNetwork>('/api/YouTrack/issue-network');
  }
}
