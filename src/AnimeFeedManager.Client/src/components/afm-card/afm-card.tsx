import { Component, h, Prop } from '@stencil/core';
import { alertController } from '@ionic/core';
import { SubscribedFeed, SubscriptionStatus } from '../../models';


@Component({
  tag: 'afm-card',
  styleUrl: 'afm-card.scss',
  shadow: true
})
export class AfmCard {
  @Prop() feedInfo: SubscribedFeed;
  @Prop() subscriptionStatus: SubscriptionStatus = SubscriptionStatus.none;
  imageUrl(url: string) {
    return !!url ? url : '/assets/img/no_available.jpg';
  }

  async handleSubscription() {
    const alert = await alertController.create({
      header: 'Subscription Confirmation',
      message: `Do you want to subscribe to ${this.feedInfo.title}?`,
      buttons: [
        'No',
        {
          text: 'Yes',
          cssClass: 'secondary',
          handler: () => {
            console.log(this.feedInfo.feedInformation.title);
          }
        }]
    });

    await alert.present();
  }

  subcribeOption() {
    switch (this.subscriptionStatus) {
      case SubscriptionStatus.showSusbcription:
        return <ion-button
          size="small"
          class="ion-activatable ripple-parent"
          onClick={() => this.handleSubscription()}
        >
          Subscribe
        <ion-ripple-effect></ion-ripple-effect>
        </ion-button>
      case SubscriptionStatus.subscribed:
        return <ion-chip color="secondary">
          <ion-icon name="checkmark-circle-outline"></ion-icon>
          <ion-label>Subscribed</ion-label>
        </ion-chip>
      default:
        return null;
    }


  }


  render() {
    return [
      <ion-card>

        <div class="img-wrapper">
          <div class="information">
            {
              this.feedInfo.feedInformation.available
                ?
                [<ion-chip color="primary">
                  <ion-label>Feed Available</ion-label>
                </ion-chip>,
                this.subcribeOption()
                ]
                :
                <ion-chip color="warning">
                  <ion-label>Feed Not Available</ion-label>
                </ion-chip>
            }
          </div>
          {
            this.feedInfo.url ?
              <img src={this.imageUrl(this.feedInfo.url)} title={this.feedInfo.title} /> :
              <div class="placeholder">
                <p>Not Available</p>
              </div>
          }
        </div>
        <div class="content-wrapper">
          <ion-card-header>
            <ion-card-subtitle>{this.feedInfo.feedInformation.available}</ion-card-subtitle>
            <ion-card-title>{this.feedInfo.title}</ion-card-title>
          </ion-card-header>

          <ion-card-content>
            {this.feedInfo.synopsis}
          </ion-card-content>
        </div>
      </ion-card>
    ];
  }

}
