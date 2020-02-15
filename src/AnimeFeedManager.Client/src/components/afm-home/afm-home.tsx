import { Component, h } from '@stencil/core';
import { menuController } from '@ionic/core';

@Component({
  tag: 'afm-home',
  styleUrl: 'afm-home.scss'
})
export class AfmHome {

  onCLick() {
    console.log('clicked');
    menuController.toggle();
  }


  render() {
    return [
      <stencil-route-title pageTitle="Home" />,
      <ion-content class="ion-padding">
        <p>
          Welcome to the PWA Toolkit. You can use this starter to build entire
          apps with web components using Stencil and ionic/core! Check out the
          README for everything that comes in this starter out of the box and
          check out our docs on <a href="https://stenciljs.com">stenciljs.com</a> to get started.
        </p>

        {/* <ion-button href="/profile/ionic" expand="block">Profile page</ion-button> */}
        <stencil-route-link url='/profile/stencil'>
          <button>
            Profile page
          </button>
        </stencil-route-link>
        <ion-content class="ion-padding">
          <ion-button expand="block" onClick={this.onCLick}>Open Menu</ion-button>
        </ion-content>
      </ion-content>

    ];
  }
}
