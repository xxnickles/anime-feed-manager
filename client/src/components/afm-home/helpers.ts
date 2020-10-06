import { AvailableFilters } from "../afm-filters/filters";
import { SubscribedFeed } from "../../models";

const availableFilter = (anime: SubscribedFeed) => anime.feedInformation.available;
const notAvailableFilter = (anime: SubscribedFeed) => !availableFilter(anime);
const subscritionFilter = (anime: SubscribedFeed) =>
  (subscriptions: string[]) =>
    anime.feedInformation.available
    && subscriptions.includes(anime.feedInformation.title);



const completedFilter = (anime: SubscribedFeed) => anime.feedInformation.completed;

const interestedFilter = (anime: SubscribedFeed) => (interested: string[]) => interested.includes(anime.title);


type SimpleFilter = (anime: SubscribedFeed) => boolean;
type ComposedFilter = (anime: SubscribedFeed) => (subs: string[]) => boolean;

let filtersMap = new Map<AvailableFilters, SimpleFilter | ComposedFilter>();
filtersMap.set(AvailableFilters.available, availableFilter);
filtersMap.set(AvailableFilters.subscribed, subscritionFilter);
filtersMap.set(AvailableFilters.noAvailable, notAvailableFilter);
filtersMap.set(AvailableFilters.completed, completedFilter);
filtersMap.set(AvailableFilters.interested, interestedFilter)


function generateFilterFunction(filters: AvailableFilters[], subscriptions: string[], interestedList: string[]) {

  return (anime: SubscribedFeed) => filters.reduce((acc, curr) => {
    const filter = filtersMap.get(curr)
    if (filter !== undefined) {
      const initialFilter = filter(anime);
      const finalFilter = initialFilter instanceof Function ?
        curr === AvailableFilters.subscribed ?
          initialFilter(subscriptions) : initialFilter(interestedList)
        : initialFilter;
      return acc && finalFilter;
    }
    return acc;

  }, true)

}

function filterCollection(
  animes: SubscribedFeed[],
  subscriptions: string[],
  interestedList: string[],
  filters: AvailableFilters[],
) {
  return animes.filter(generateFilterFunction(filters, subscriptions, interestedList))
}

export {
  generateFilterFunction,
  filterCollection
}

