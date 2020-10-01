import { Component, h, State, Listen, Prop } from '@stencil/core';
import {
  fetchLatestSeason,
  animeListQuery,
  UserInfo,
  userInfoQuery,
  addSubscription,
  fetchSeasonCollection,
  unsubscribe,
  addInterested,
  removeInterested
} from '../../stores';
import { SubscribedFeed, SubscriptionStatus, Season, InterestedStatus } from '../../models';
import { untilDestroyed } from '../../utils';
import { AvailableFilters } from '../afm-filters/filters';
import { filterCollection } from './helpers';
@Component({
  tag: 'afm-home',
  styleUrl: 'afm-home.scss',
  shadow: true
})
export class AfmHome {
  @Prop() year: string;
  @Prop() season: Season;
  @State() state = {
    animes: [] as SubscribedFeed[],
    userInfo: {
      logged: false,
      userName: '',
      subscriptions: [],
      interested: []
    } as UserInfo,
    selectedFilters: [] as AvailableFilters[]
  };

  animeCollection = [] as SubscribedFeed[];

  componentWillLoad() {
    animeListQuery.animes$
      .pipe(untilDestroyed(this))
      .subscribe(animes => {
        this.state = {
          ...this.state,
          animes: filterCollection(
            animes,
            this.state.userInfo.subscriptions,
            this.state.selectedFilters)
        }
        this.animeCollection = animes;
      });

    userInfoQuery.userInfo$
      .pipe(untilDestroyed(this))
      .subscribe(userInfo => {
        this.state = { ...this.state, ...{ userInfo } };
      });

    if (!!this.year && !!this.season) {
      fetchSeasonCollection(Season[this.season.toLocaleLowerCase()], parseInt(this.year));
    } else {
      fetchLatestSeason();
    }
  }

  getSubscriptionStatus(anime: SubscribedFeed) {
    if (!this.state.userInfo.logged || !anime.feedInformation.available) return SubscriptionStatus.none;

    return this.state.userInfo.subscriptions.includes(anime.feedInformation.title) ?
      SubscriptionStatus.subscribed :
      SubscriptionStatus.showSusbcription;

  }

  getInterestedStatus(anime: SubscribedFeed) {
    if (!this.state.userInfo.logged) return InterestedStatus.none;
    return this.state.userInfo.interested.includes(anime.title) ?
      InterestedStatus.interested :
      InterestedStatus.showInterested;
  }

  @Listen('subscriptionSelected')
  subscriptionSelectedHandler(event: CustomEvent<string>) {
    addSubscription({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  @Listen('unsubscriptionSelected')
  unsubscriptionSelectedHandler(event: CustomEvent<string>) {
    unsubscribe({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  @Listen('interestedSelected')
  interestedSelectedHandler(event: CustomEvent<string>) {
    addInterested({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  @Listen('removeInterestedSelected')
  removeInterestedSelectedSelectedHandler(event: CustomEvent<string>) {
    removeInterested({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  @Listen('filterChanged')
  filtersHandler(event: CustomEvent<AvailableFilters[]>) {
    // Solves re-rendering issue when user change and filter by susbscriptions is active
    setTimeout(() => {
      this.state = {
        ...this.state,
        animes: filterCollection(this.animeCollection, this.state.userInfo.subscriptions, event.detail),
        selectedFilters: event.detail
      }
    }, 0);
  }

  render() {
    return [
      <afm-filters authenticated={this.state.userInfo.logged}></afm-filters>,
      <ion-content class="ion-padding grid">
        {
          this.state.animes.map((anime) =>
            (<afm-card
              feedInfo={anime}
              subscriptionStatus={this.getSubscriptionStatus(anime)}
              interestedStatus={this.getInterestedStatus(anime)}
            ></afm-card>)
          )}

      </ion-content>

    ];
  }
}
