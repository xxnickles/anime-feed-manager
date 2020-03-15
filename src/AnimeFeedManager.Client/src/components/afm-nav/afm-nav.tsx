import { Component, h, Listen, State } from '@stencil/core';
import { updateUser, userInfoQuery } from '../../stores';
import { untilDestroyed } from '../../utils';


@Component({
  tag: 'afm-nav',
  styleUrl: 'afm-nav.scss'
})
export class AfmNav {

  @State() userInfo = {
    logged: false,
    userName: ''
  }

  @Listen('emailSelected')
  emailSelectedHandler(event: CustomEvent<string>) {
    updateUser(event.detail);
  }

  componentWillLoad() {
    userInfoQuery.userLogginInfo$
      .pipe(untilDestroyed(this))
      .subscribe(userInfo => {
        this.userInfo = { ...this.userInfo, ...userInfo };
        console.log('nave user info', this.userInfo);
      });
  }

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
            {
              !this.userInfo.logged ?
                <afm-mail-selector ></afm-mail-selector> :
                <ion-item >
                  <ion-icon name="person" slot="start"></ion-icon>
                  <ion-label>{this.userInfo.userName}</ion-label>
                </ion-item >
            }

            <ion-menu-toggle>
              <stencil-route-link url='/' activeClass='active' exact={true}>
                <ion-item button >
                  <ion-icon name="home" slot="start"></ion-icon>
                  <ion-label>Home</ion-label>
                </ion-item>
              </stencil-route-link>
            </ion-menu-toggle>
          </ion-list>
        </ion-content>
      </ion-menu>
    ]
  }
}
