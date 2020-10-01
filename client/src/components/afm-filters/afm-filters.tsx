import { Event, Component, h, Prop, State, EventEmitter, Watch } from '@stencil/core';
import { AvailableFilters } from './filters';


@Component({
  tag: 'afm-filters',
  styleUrl: 'afm-filters.scss',
  shadow: true,
})
export class AfmFilters {
  @Prop() authenticated = false;
  @State() selectedFilters: AvailableFilters[] = [];
  @Event() filterChanged: EventEmitter<AvailableFilters[]>;

  processFilter(filter: AvailableFilters) {
    if (this.selectedFilters.includes(filter)) {
      this.selectedFilters = this.selectedFilters.filter(x => x !== filter);
    } else {
      this.selectedFilters = [...this.selectedFilters, filter];
    }
    this.filterChanged.emit(this.selectedFilters);
  }

  @Watch('authenticated')
  watchHandler(newValue: boolean) {
    if (!newValue && this.selectedFilters.includes(AvailableFilters.subscribed)) {
      this.selectedFilters = this.selectedFilters.filter(x => x !== AvailableFilters.subscribed);
      this.filterChanged.emit(this.selectedFilters);
    }
  }

  calculateColor(filterType: AvailableFilters) {
    return this.selectedFilters.includes(filterType) ? 'primary' : 'default'
  }

  renderOptionalFilters() {
    if (this.authenticated) {
      return [
        <ion-chip
          color={this.calculateColor(AvailableFilters.subscribed)}
          onClick={() => this.processFilter(AvailableFilters.subscribed)}>
          <ion-icon name="bookmarks-outline" color={this.calculateColor(AvailableFilters.subscribed)}></ion-icon>
          <ion-label color={this.calculateColor(AvailableFilters.subscribed)}>Subscribed</ion-label>
        </ion-chip>,
        <ion-chip
          color={this.calculateColor(AvailableFilters.interested)}
          onClick={() => this.processFilter(AvailableFilters.interested)}>
          <ion-icon name="bookmarks-outline" color={this.calculateColor(AvailableFilters.interested)}></ion-icon>
          <ion-label color={this.calculateColor(AvailableFilters.interested)}>Interested</ion-label>
        </ion-chip>
      ]
    }
  }

  render() {
    return [
      <ion-chip
        color={this.calculateColor(AvailableFilters.available)}
        onClick={() => this.processFilter(AvailableFilters.available)}>
        <ion-icon name="checkmark-circle" color={this.calculateColor(AvailableFilters.available)}></ion-icon>
        <ion-label color={this.calculateColor(AvailableFilters.available)}>Feed Available</ion-label>
      </ion-chip>,
      <ion-chip
        color={this.calculateColor(AvailableFilters.noAvailable)}
        onClick={() => this.processFilter(AvailableFilters.noAvailable)}>
        <ion-icon name="close-circle-outline" color={this.calculateColor(AvailableFilters.noAvailable)}></ion-icon>
        <ion-label color={this.calculateColor(AvailableFilters.noAvailable)}>No Feed Available</ion-label>
      </ion-chip>,
      <ion-chip
        color={this.calculateColor(AvailableFilters.completed)}
        onClick={() => this.processFilter(AvailableFilters.completed)}>
        <ion-icon name="trophy-outline" color={this.calculateColor(AvailableFilters.completed)}></ion-icon>
        <ion-label color={this.calculateColor(AvailableFilters.completed)}>Show Ended</ion-label>
      </ion-chip>,
      this.renderOptionalFilters()
    ];
  }

}
