export interface SubscribedFeed {
  id: string,
  title: string,
  url: string,
  synopsis: string,
  feedInformation: FeedInfo
}

export interface FeedInfo {
  available: boolean,
  completed:boolean,
  title: string
}
