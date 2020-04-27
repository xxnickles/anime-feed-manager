import { Component, h, Listen, State } from '@stencil/core';
import { updateUser, userInfoQuery, tryGetUserFromStorage, logOut } from '../../stores';
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
      });
    tryGetUserFromStorage();
  }

  handleEmailSent() {
    logOut();
  }

  renderUserInfo() {
    return [
      <ion-item >
        <ion-icon name="person" slot="start"></ion-icon>
        <ion-label>{this.userInfo.userName}</ion-label>
      </ion-item >,
      <ion-item >
        <ion-button
          size="small"
          mode="ios"
          onClick={() => this.handleEmailSent()}
        >Change User</ion-button>
      </ion-item >
    ]
  }

  render() {
    return [
      <ion-menu side="start" contentId="menu-content">
        <ion-header>
          <ion-toolbar color="primary">
            <ion-title>Menu</ion-title>
          </ion-toolbar>
        </ion-header>
        <ion-content >
          <ion-list>
            {
              !this.userInfo.logged ?
                <afm-mail-selector ></afm-mail-selector> :
                this.renderUserInfo()
            }

            {/* <ion-menu-toggle>
              <ion-router-link href='/'>
                <ion-item button >
                  <ion-icon name="home" slot="start"></ion-icon>
                  <ion-label>Home</ion-label>
                </ion-item>
              </ion-router-link>
            </ion-menu-toggle> */}
          </ion-list>
        </ion-content>
      </ion-menu>
    ]
  }
}
