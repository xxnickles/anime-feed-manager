import { Query } from '@datorama/akita';
import { SeasonCollectionState, animeListStore, AnimeListStore } from './anime-list.store';

export class AnimeListQuery extends Query<SeasonCollectionState> {
  seasonInformation$ = this.select(state => state.seasonInfo);
  animes$ = this.select(state => state.animes);
  availableSeasons$ = this.select(state => state.availableSeasons);
  get isLatestSeason() {
    var currentState = this.getValue();
    var latestSeason = currentState.availableSeasons[0];
    return currentState.seasonInfo === 'latest' ||
      (currentState.seasonInfo.season === latestSeason.season &&
        currentState.seasonInfo.year === latestSeason.year)
  }

  constructor(protected store: AnimeListStore) {
    super(store);
  }

}

export const animeListQuery = new AnimeListQuery(animeListStore);
