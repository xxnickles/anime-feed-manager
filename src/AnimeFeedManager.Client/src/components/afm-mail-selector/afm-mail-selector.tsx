import {  Event, EventEmitter, Component, h, State } from '@stencil/core';

@Component({
  tag: 'afm-mail-selector',
  styleUrl: 'afm-mail-selector.scss'
})
export class AfmMailSelector {

  @State() value: string;
  @State() valid = false;
  @Event() emailSelected: EventEmitter<string>;

  handleChangeValue(event: InputEvent) {
    const target = event.target as HTMLInputElement;
    this.valid = target.validity.valid;
    if (this.valid) {
      this.value = target.value;
    }
  }

  handleEmailSent() {
    this.emailSelected.emit(this.value);
  }

  render() {
    return [
      <ion-item >
        <ion-label position="floating">Your Email</ion-label>
        <ion-input
          inputMode="email"
          type="email"
          value={this.value}
          onInput={(ev: InputEvent) => this.handleChangeValue(ev)}
          required
        ></ion-input>
      </ion-item >,
      <ion-item >
        <ion-button
          size="small"
          disabled={!this.valid}
          onClick={() => this.handleEmailSent()}
        >Send</ion-button>
      </ion-item >
    ]
  }
}
