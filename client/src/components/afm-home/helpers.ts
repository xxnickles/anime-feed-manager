import { AvailableFilters } from "../afm-filters/filters";
import { SubscribedFeed } from "../../models";

const availableFilter = (anime: SubscribedFeed) => anime.feedInformation.available;
const notAvailableFilter = (anime: SubscribedFeed) => !availableFilter(anime);
const subscritionFilter = (anime: SubscribedFeed) =>
  (subscriptions: string[]) =>
    anime.feedInformation.available
    && subscriptions.includes(anime.feedInformation.title);

type SimpleFilter = (anime: SubscribedFeed) => boolean;
type ComposedFilter = (anime: SubscribedFeed) => (subs: string[]) => boolean;
let filtersMap = new Map<AvailableFilters, SimpleFilter | ComposedFilter>();
filtersMap.set(AvailableFilters.available, availableFilter);
filtersMap.set(AvailableFilters.subscribed, subscritionFilter);
filtersMap.set(AvailableFilters.noAvailable, notAvailableFilter);


function generateFilterFunction(filters: AvailableFilters[], subscriptions: string[]) {

  return (anime: SubscribedFeed) => filters.reduce((acc, curr) => {
    const filter = filtersMap.get(curr)
    if (filter !== undefined) {
      const initialFilter = filter(anime);
      const finalFilter = initialFilter instanceof Function ?
        initialFilter(subscriptions) : initialFilter;
      return acc && finalFilter;
    }

    return acc;

  }, true)

}

function filterCollection(
  animes: SubscribedFeed[],
  subscriptions: string[],
  filters: AvailableFilters[]) {
  return animes.filter(generateFilterFunction(filters, subscriptions))
}

export {
  generateFilterFunction,
  filterCollection
}

