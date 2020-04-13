import { Component, h, State, Listen, Prop } from '@stencil/core';
import {
  fetchLatestSeason,
  animeListQuery,
  UserInfo,
  userInfoQuery,
  addSubscription,
  fetchSeasonCollection,
  unsubscribe
} from '../../stores';
import { SubscribedFeed, SubscriptionStatus, Season } from '../../models';
import { untilDestroyed } from '../../utils';
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
      subscriptions: []
    } as UserInfo,

  };

  componentWillLoad() {
    animeListQuery.animes$
      .pipe(untilDestroyed(this))
      .subscribe(animes => {
        this.state = { ...this.state, ...{ animes } }
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

  @Listen('subscriptionSelected')
  subscriptionSelectedHandler(event: CustomEvent<string>) {
    addSubscription({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  @Listen('unsubscriptionSelected')
  unsubscriptionSelectedHandler(event: CustomEvent<string>) {
    unsubscribe({ animeId: event.detail, subscriber: this.state.userInfo.userName });
  }

  render() {
    return [
      <ion-content class="ion-padding grid">
        {
          this.state.animes.map((anime) =>
            (<afm-card
              feedInfo={anime}
              subscriptionStatus={this.getSubscriptionStatus(anime)} ></afm-card>)
          )}
      </ion-content>

    ];
  }
}
