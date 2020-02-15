import { Component, h } from '@stencil/core';


@Component({
  tag: 'afm-nav',
  styleUrl: 'afm-nav.scss'
})
export class AfmNav {



  render() {
    return [
      <ion-menu side="start" contentId="navigation">
        <ion-header>
          <ion-toolbar color="primary">
            <ion-title>Menu</ion-title>
          </ion-toolbar>
        </ion-header>
        <ion-content>
          <ion-list>
            <ion-menu-toggle>
              <stencil-route-link url='/' activeClass='active' exact={true}>
                <ion-item button>
                  <ion-icon name="home" slot="start"></ion-icon>
                  <ion-label>Home</ion-label>
                </ion-item>
              </stencil-route-link>
            </ion-menu-toggle>

            <ion-menu-toggle>
              <stencil-route-link url='/profile/stencil' activeClass='active'>
                <ion-item button>
                  <ion-icon name="person" slot="start"></ion-icon>
                  <ion-label>Profile</ion-label>
                </ion-item>
              </stencil-route-link>
            </ion-menu-toggle>
          </ion-list>
        </ion-content>
      </ion-menu>
    ]
  }
}
