import { baseApi } from '../../base';
import { userStore, UserState, emptyUserState } from './user.store'
import { presentToast, safeRequestExecute, safeRequestExecuteWithLoader } from '../../helpers';
import { Subscription } from '../../models';

function updateUser(user: string) {
  safeRequestExecuteWithLoader(async () => {
    const subscriptions = await baseApi.get(`subscriptions/${user}`)
      .json<string[]>();

    const interested = await baseApi.get(`interested/${user}`)
      .json<string[]>();

    const newState: UserState = {
      currentUser: user,
      isLogged: true,
      subscriptions,
      interested
    }
    userStore.update(newState);
    localStorage.setItem('user', user);
  })
}


function addSubscription(subscription: Subscription) {
  safeRequestExecute(async () => {
    await baseApi.post(`subscriptions`, { json: subscription });
    userStore.update(state => {
      return { ...state, subscriptions: [...state.subscriptions, subscription.animeId] }
    })
    presentToast({
      color: 'success',
      duration: 3000,
      message: `${subscription.animeId} has been added to your subscriptions`
    });
  })
}

function unsubscribe(subscription: Subscription) {
  safeRequestExecute(async () => {
    await baseApi.post(`unsubscribe`, { json: subscription });
    userStore.update(state => {
      return { ...state, subscriptions: state.subscriptions.filter(subs => subs !== subscription.animeId) }
    })
    presentToast({
      color: 'success',
      duration: 3000,
      message: `${subscription.animeId} has been removed of your subscriptions`
    });
  })
}

function addInterested(subscription: Subscription) {
  safeRequestExecute(async () => {
    await baseApi.post(`interested`, { json: subscription });
    userStore.update(state => {
      return { ...state, interested: [...state.interested, subscription.animeId] }
    })
    presentToast({
      color: 'success',
      duration: 3000,
      message: `${subscription.animeId} has been added to your interested list`
    });
  })
}

function removeInterested(subscription: Subscription) {
  safeRequestExecute(async () => {
    await baseApi.post(`removeInterested`, { json: subscription });
    userStore.update(state => {
      return { ...state, interested: state.interested.filter(subs => subs !== subscription.animeId) }
    })
    presentToast({
      color: 'success',
      duration: 3000,
      message: `${subscription.animeId} has been removed of your interested list`
    });
  })
}


function tryGetUserFromStorage() {
  const user = localStorage.getItem('user');
  if (user) {
    updateUser(user);
  }
}

function logOut() {
  userStore.update(emptyUserState());
  localStorage.removeItem('user');
}

export {
  updateUser,
  tryGetUserFromStorage,
  logOut,
  addSubscription,
  unsubscribe,
  addInterested,
  removeInterested
}
