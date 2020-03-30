import { Season } from './season';
import { SubscribedFeed } from './feed';

interface SeasonCollection {
  season: Season;
  year: number;
  animes: SubscribedFeed[]
}

export {
  SeasonCollection
}
