import { Store, StoreConfig } from '@datorama/akita';
import { SubscribedFeed, SeasonInformation } from '../../models';

type SeasonInfo = SeasonInformation | 'latest';

interface SeasonCollectionState {
  seasonInfo: SeasonInfo;
  animes: SubscribedFeed[],
  availableSeasons: SeasonInformation[]

}

function createInitialState(): SeasonCollectionState {
  return {
    seasonInfo: 'latest',
    animes: [],
    availableSeasons: []
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
  animeListStore,
  SeasonInfo
}


