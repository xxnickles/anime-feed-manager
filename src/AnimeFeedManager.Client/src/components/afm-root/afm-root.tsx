import { Component, h } from '@stencil/core';
{/* ref: https://github.com/ionic-team/ionic-stencil-hn-app/blob/master/src/components/ionic-hn/ionic-hn.tsx */ }
@Component({
  tag: 'afm-root',
  styleUrl: 'afm-root.scss'
})
export class AfmRoot {

  render() {
    return [
      <ion-header>
        <ion-toolbar color="primary">
          <ion-buttons slot="start">
            <ion-menu-button></ion-menu-button>
          </ion-buttons>
          {/* <ion-title>Home</ion-title> */}
        </ion-toolbar>
      </ion-header>,

      <stencil-router id='navigation'>
        <stencil-route url='/' component='afm-home' exact={true} title={"Home"} />
        <stencil-route url='/profile/:name' component='app-profile' title={"Profile"} />
      </stencil-router>,
      <afm-nav></afm-nav>
    ];
  }
}
