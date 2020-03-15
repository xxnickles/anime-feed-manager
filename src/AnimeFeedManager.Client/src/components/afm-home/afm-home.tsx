import { Component, h, State } from '@stencil/core';
import { menuController } from '@ionic/core';
import { fetchLatestSeason, animeListQuery, UserInfo, userInfoQuery } from '../../stores';
import { SubscribedFeed, SubscriptionStatus } from '../../models';
import { untilDestroyed } from '../../utils';
@Component({
  tag: 'afm-home',
  styleUrl: 'afm-home.scss',
  shadow: true
})
export class AfmHome {

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
      .subscribe(animes => this.state = { ...this.state, ...{ animes } });

    userInfoQuery.userInfo$
      .pipe(untilDestroyed(this))
      .subscribe(userInfo => {
        this.state = { ...this.state, ...{ userInfo } };
      });

    fetchLatestSeason();
  }

  onCLick() {
    menuController.toggle();
  }

  getSubscriptionStatus(anime: SubscribedFeed) {
    if (!this.state.userInfo.logged || !anime.feedInformation.available) return SubscriptionStatus.none;

    return this.state.userInfo.subscriptions.includes(anime.feedInformation.title) ?
      SubscriptionStatus.subscribed :
      SubscriptionStatus.showSusbcription;

  }


  render() {
    return [
      <stencil-route-title pageTitle="Home" />,
      <ion-content class="ion-padding grid">
        {
          this.state.animes.map((anime) =>
            <afm-card
              feedInfo={anime}
              subscriptionStatus={this.getSubscriptionStatus(anime)} ></afm-card>
          )}


        {/* <ion-button href="/profile/ionic" expand="block">Profile page</ion-button> */}
        {/* <stencil-route-link url='/profile/stencil'>
          <button>
            Profile page
          </button>
        </stencil-route-link>
        <ion-content class="ion-padding">
          <ion-button expand="block" onClick={this.onCLick}>Open Menu</ion-button>
        </ion-content> */}
      </ion-content>

    ];
  }
}
