
import { Season } from '../../models';
import { baseApi } from '../../base';
import { SeasonCollectionState, animeListStore } from './anime-list.store';

async function fetchSeasonCollection(season: Season, year: number) {
  const result = await baseApi.get(`library/${year}/${season}`)
    .json<SeasonCollectionState>();
  animeListStore.update(result);
}

async function fetchLatestSeason() {
  const result = await baseApi.get(`library/latest`)
    .json<SeasonCollectionState>();
  animeListStore.update(result);
}

export {
  fetchSeasonCollection,
  fetchLatestSeason
}

// export const animeListService = new AnimeListService(animeListStore);
