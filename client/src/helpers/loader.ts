import { BehaviorSubject } from 'rxjs';
import { loadingController } from '@ionic/core';
import { skip, delay } from 'rxjs/operators';

type LoaderEvent = {
  values: string[],
  running: boolean
}

class LoaderService {
  private loaderState = new BehaviorSubject<LoaderEvent>({ values: [], running: false });
  private loaderstate$ = this.loaderState.asObservable();
  private loader: HTMLIonLoadingElement;

  constructor() {
    this.loaderstate$
      .pipe(skip(1), delay(100))
      .subscribe(async val => {
        if (val.values.length === 1 && !val.running) {
          if (!this.hasLoaderInstance()) {
            await this.createLoader();
            this.loader.present();
          }
          this.updateLoaderRunningState(true);
        }

        if (val.values.length === 0 && this.hasLoaderInstance()) {
          this.loader.dismiss();
          this.loader = null;
          this.updateLoaderRunningState(false);
        }
      });
  }


  enqueeLoaderEvent() {
    const current = this.loaderState.getValue();
    this.loaderState.next({ ...current, values: [...current.values, 'EVENT'] });
  }

  removeLoaderEvent() {
    const current = this.loaderState.getValue();
    const newValue = current.values.slice(0);
    newValue.pop();
    this.loaderState.next({ ...current, values: newValue });
  }

  private updateLoaderRunningState(value: boolean) {
    const current = this.loaderState.getValue();
    this.loaderState.next({ ...current, running: value });
  }

  private async createLoader() {
    this.loader = await loadingController.create({
      message: `LOADING`
    });
  }

  private hasLoaderInstance = () => this.loader !== null && this.loader !== undefined;
}

const loaderService = new LoaderService();

export {
  loaderService
}
