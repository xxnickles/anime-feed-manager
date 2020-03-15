import { Store, StoreConfig } from '@datorama/akita';
import { Season, SubscribedFeed } from '../../models';

interface SeasonCollectionState {
  season: Season;
  year: number;
  animes: SubscribedFeed[]
}

function createInitialState(): SeasonCollectionState {
  return {
    season: Season.spring,
    year: new Date().getFullYear(),
    animes: []
  };
}

@StoreConfig({ name: 'animeList' })
class AnimeListStore extends Store<SeasonCollectionState> {

  constructor() {
    super(createInitialState());
  }
}

const animeListStore = new AnimeListStore();

export {
  AnimeListStore,
  SeasonCollectionState,
  animeListStore
}


