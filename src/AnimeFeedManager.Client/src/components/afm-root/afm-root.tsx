import { Component, h, State, } from '@stencil/core';
import { fetchAvailableSeasons, animeListQuery, fetchLatestSeason, fetchSeasonCollection } from '../../stores';
import { untilDestroyed } from '../../utils';
import { SeasonInformation, Season } from '../../models';
import { SelectChangeEventDetail } from '@ionic/core';
{/* ref: https://github.com/ionic-team/ionic-stencil-hn-app/blob/master/src/components/ionic-hn/ionic-hn.tsx */ }

@Component({
  tag: 'afm-root',
  styleUrl: 'afm-root.scss'
})
export class AfmRoot {

  @State() seasonInformation: SeasonInformation[] = [];

  componentWillLoad() {
    fetchAvailableSeasons();


    animeListQuery.availableSeasons$
      .pipe(untilDestroyed(this))
      .subscribe(seasons => {
        this.seasonInformation = seasons;
      });
  }

  renderRouter() {
    return (
      <ion-router useHash={false}>
        <ion-route-redirect from="/" to='/latest' />
        <ion-route url="/:year/:season" component="afm-home" ></ion-route>
        <ion-route url="/latest" component="afm-home"></ion-route>
      </ion-router >
    )
  }

  onSeasonChange(eventValue: CustomEvent<SelectChangeEventDetail>) {
    const seasonInfo = eventValue.detail.value;
    let ionRouterElement = document.querySelector('ion-router');
    if (seasonInfo === 'latest') {
      fetchLatestSeason();
      ionRouterElement.push(`/`);
    } else {
      fetchSeasonCollection(Season[seasonInfo.season.toLocaleLowerCase()], parseInt(seasonInfo.year));
      ionRouterElement.push(`${seasonInfo.year}/${seasonInfo.season}`);
    }
  }


  render() {
    return (
      <ion-app>
        {this.renderRouter()}
        <afm-nav></afm-nav>
        <ion-header>
          <ion-toolbar color="primary">
            <ion-buttons slot="start">
              <ion-menu-button></ion-menu-button>
            </ion-buttons>

            <ion-select interface="action-sheet" placeholder="Select Season" slot="end" onIonChange={val => this.onSeasonChange(val)}>
              <ion-select-option value="latest">latest</ion-select-option>
              {
                this.seasonInformation.map((seasonInfo) => (
                  <ion-select-option value={seasonInfo}>
                    {`${seasonInfo.year} ${seasonInfo.season}`}</ion-select-option>
                ))
              })
            </ion-select>

          </ion-toolbar>
        </ion-header>
        <ion-router-outlet id="menu-content"></ion-router-outlet>
      </ion-app>

    );
  }
}

