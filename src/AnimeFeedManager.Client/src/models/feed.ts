export interface SubscribedFeed {
  id: string,
  title: string,
  url: string,
  synopsis: string,
  feedInformation: FeedInfo,
  subscribed: boolean
}

export interface FeedInfo {
  available: boolean,
  title: string
}
