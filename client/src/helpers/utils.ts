import ky from 'ky';
import { toastController, ToastOptions } from '@ionic/core';
import { loaderService } from './loader';

async function safeRequestExecuteWithLoader(fun: () => Promise<void>) {

  try {
    loaderService.enqueeLoaderEvent();
    await fun();
  } catch (error) {
    processError(error);
  } finally {
    loaderService.removeLoaderEvent();
  }
}

async function safeRequestExecute(fun: () => Promise<void>) {
  try {
    await fun();
  } catch (error) {
    processError(error);
  }
}

async function processError(error) {
  await presentToast({
    color: 'danger',
    duration: 3000,
    message: parseError(error),
  });
}

async function presentToast(options: ToastOptions) {
  const toast = await toastController.create(options);
  await toast.present();
}

function parseError(error) {
  if (error instanceof ky.HTTPError) {
    return `An error has ocurred: ${error.message}`;
  } else if (error instanceof ky.TimeoutError) {
    return `Request has timed out`;
  } else {
    return `An error ocurred: ${error}`;
  }
}

export {
  presentToast,
  safeRequestExecuteWithLoader,
  safeRequestExecute
}
