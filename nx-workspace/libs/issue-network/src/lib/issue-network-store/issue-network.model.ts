export interface IssueLinkModel {
  type: string,
  source: string,
  target: string,
}

export interface IssueModel {
  id: string,
  projectId: string,
  summary: string,
  links: IssueLinkModel[],
  topId: string,
  level: number,
}

export interface IssueNetworkSearchOptions {
  project: string[],
}

export interface IssueNetwork {
  options: IssueNetworkSearchOptions,
  issues: IssueModel[],
  links: IssueLinkModel[],
}
