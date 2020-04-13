import '@ionic/core';
import { Build } from "@stencil/core";
import { akitaDevtools } from '@datorama/akita';
// import { setupConfig } from '@ionic/core';
if (Build.isDev) {
  akitaDevtools();
}

export default () => {
  // setupConfig({
  //   mode: 'ios'
  // });
};
