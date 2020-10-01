
import { Season, SeasonInformation, SeasonCollection } from '../../models';
import { baseApi } from '../../base';
import { animeListStore } from './anime-list.store';
import { safeRequestExecuteWithLoader } from '../../helpers';

function fetchSeasonCollection(season: Season, year: number) {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`library/${year}/${season}`)
      .json<SeasonCollection>();
    animeListStore.update(state => ({
      ...state,
      ...{
        animes: result.animes,
        seasonInfo: {
          season: result.season,
          year: result.year
        }
      }
    }));
  })
}

function fetchLatestSeason() {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`library/latest`)
      .json<SeasonCollection>();
    animeListStore.update(state => ({
      ...state,
      ...{
        animes: result.animes,
        seasonInfo: 'latest'
      }
    }));
  });
}

function fetchAvailableSeasons() {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`seasons`)
      .json<SeasonInformation[]>();
    animeListStore.update(state => ({
      ...state,
      availableSeasons: result,
    }));
  })
}


export {
  fetchSeasonCollection,
  fetchLatestSeason,
  fetchAvailableSeasons
}
