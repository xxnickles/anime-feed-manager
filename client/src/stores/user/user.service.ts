import { baseApi } from '../../base';
import { userStore, UserState, emptyUserState } from './user.store'
import { safeRequestExecuteWithLoader } from '../../helpers';

function updateUser(user: string) {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`subscriptions/${user}`)
      .json<string[]>();

    const newState: UserState = {
      currentUser: user,
      isLogged: true,
      subscriptions: result
    }
    userStore.update(newState);
    localStorage.setItem('user', user);
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
  logOut
}
