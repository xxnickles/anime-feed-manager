import { Query } from '@datorama/akita';
import { SeasonCollectionState, animeListStore, AnimeListStore } from './anime-list.store';

export class AnimeListQuery extends Query<SeasonCollectionState> {
  season$ = this.select(state => state.season);
  year$ = this.select(state => state.year);
  animes$ = this.select(state => state.animes);

  constructor(protected store: AnimeListStore) {
    super(store);
  }

}

export const animeListQuery = new AnimeListQuery(animeListStore);
