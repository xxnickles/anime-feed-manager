import { Event, Component, h, Prop, EventEmitter } from '@stencil/core';
import { alertController } from '@ionic/core';
import { InterestedStatus, SubscribedFeed, SubscriptionStatus } from '../../models';

@Component({
  tag: 'afm-card',
  styleUrl: 'afm-card.scss',
  shadow: true
})
export class AfmCard {
  @Prop() feedInfo: SubscribedFeed;
  @Prop() subscriptionStatus: SubscriptionStatus = SubscriptionStatus.none;
  @Prop() interestedStatus: InterestedStatus = InterestedStatus.none;
  @Event() subscriptionSelected: EventEmitter<string>;
  @Event() unsubscriptionSelected: EventEmitter<string>;
  @Event() interestedSelected: EventEmitter<string>;
  @Event() removeInterestedSelected: EventEmitter<string>;

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
            this.subscriptionSelected.emit(this.feedInfo.feedInformation.title);
          }
        }]
    });

    await alert.present();
  }

  async handleUnsubscription() {
    const alert = await alertController.create({
      header: 'Unsubscription Confirmation',
      message: `Do you want to unsubscribe to ${this.feedInfo.title}?`,
      buttons: [
        'No',
        {
          text: 'Yes',
          cssClass: 'secondary',
          handler: () => {
            this.unsubscriptionSelected.emit(this.feedInfo.feedInformation.title);
          }
        }]
    });

    await alert.present();
  }


  async handleAddInterest() {
    const alert = await alertController.create({
      header: 'Add Interest Confirmation',
      message: `Do you want to add ${this.feedInfo.title} to your interested list?`,
      buttons: [
        'No',
        {
          text: 'Yes',
          cssClass: 'secondary',
          handler: () => {
            this.interestedSelected.emit(this.feedInfo.title);
          }
        }]
    });

    await alert.present();
  }

  async handleRemoveInterest() {
    const alert = await alertController.create({
      header: 'Remove Interested Confirmation',
      message: `Do you want to remove  ${this.feedInfo.title} form your interested list?`,
      buttons: [
        'No',
        {
          text: 'Yes',
          cssClass: 'secondary',
          handler: () => {
            this.removeInterestedSelected.emit(this.feedInfo.title);
          }
        }]
    });

    await alert.present();
  }

  subcribeOption() {
    switch (this.subscriptionStatus) {
      case SubscriptionStatus.showSusbcription:
        return [
          <ion-chip color="primary">
            <ion-label>Feed Available</ion-label>
          </ion-chip>,
          <ion-button
            size="small"
            class="ion-activatable ripple-parent"
            mode="ios"
            onClick={() => this.handleSubscription()}
          >
            Subscribe
        <ion-ripple-effect></ion-ripple-effect>
          </ion-button>
        ]
      case SubscriptionStatus.subscribed:
        return [
          <ion-chip color="secondary">
            <ion-icon name="checkmark-circle-outline"></ion-icon>
            <ion-label>Subscribed</ion-label>
          </ion-chip>,
          <ion-button
            size="small"
            class="ion-activatable ripple-parent"
            mode="ios"
            onClick={() => this.handleUnsubscription()}
          >
            Unsubscribe
        <ion-ripple-effect></ion-ripple-effect>
          </ion-button>
        ]
      default:
        return null;
    }
  }

  interestedOption() {
    switch (this.interestedStatus) {
      case InterestedStatus.showInterested:
        return <ion-button
          size="small"
          class="ion-activatable ripple-parent"
          mode="ios"
          onClick={() => this.handleAddInterest()}
        >
          Add to Interested
        <ion-ripple-effect></ion-ripple-effect>
        </ion-button>
      case InterestedStatus.interested:
        return [
          <ion-chip color="secondary">
            <ion-icon name="albums-outline"></ion-icon>
            <ion-label>Interested</ion-label>
          </ion-chip>,
          <ion-button
            size="small"
            class="ion-activatable ripple-parent"
            mode="ios"
            onClick={() => this.handleRemoveInterest()}
          >
            Remove from interested
        <ion-ripple-effect></ion-ripple-effect>
          </ion-button>
        ]
      default:
        return null;
    }
  }

  renderFeedInfo() {
    return !this.feedInfo.feedInformation.completed ?
      [this.subcribeOption()] :
      [
        <ion-chip color="secondary">
          <ion-icon name="trophy-outline"></ion-icon>
          <ion-label>Show Ended</ion-label>
        </ion-chip>
      ]
  }

  renderNotAvailable() {
    return [
      <ion-chip color="warning">
        <ion-label>Feed Not Available</ion-label>
      </ion-chip>,
      this.interestedOption()
    ]
  }

  parseSynopsis() {
    return (this.feedInfo.synopsis.split("\n").map(s => <p>{s}</p>));
  }

  render() {
    return [
      <ion-card>

        <div class="img-wrapper">
          <div class="information">
            {
              this.feedInfo.feedInformation.available
                ?
                this.renderFeedInfo()
                : this.renderNotAvailable()

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
            {this.parseSynopsis()}
          </ion-card-content>
        </div>
      </ion-card>
    ];
  }

}
