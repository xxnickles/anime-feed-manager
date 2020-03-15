import { baseApi } from '../../base';
import { userStore, UserState } from './user.store'

async function updateUser(user: string) {
  const result = await baseApi.get(`subscriptions/${user}`)
    .json<string[]>();

  const newState: UserState = {
    currentUser: user,
    isLogged: true,
    subscriptions: result
  }
  userStore.update(newState);

}

export {
  updateUser
}
