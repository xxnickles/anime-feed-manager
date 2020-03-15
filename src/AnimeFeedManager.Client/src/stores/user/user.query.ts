import { Query } from '@datorama/akita';
import { UserState, UserStore, userStore } from './user.store';

type UserInfo = {
  logged: boolean,
  userName: string
  subscriptions: string[]
}

class UserInfoQuery extends Query<UserState> {
  userInfo$ = this.select<UserInfo>(state => (
    {
      logged: state.isLogged,
      userName: state.currentUser,
      subscriptions: state.subscriptions
    }));

  userLogginInfo$ = this.select(state => ({
    logged: state.isLogged,
    userName: state.currentUser
  }))

  subscriptions = this.select(state => state.subscriptions);

  constructor(protected store: UserStore) {
    super(store);
  }
}

const userInfoQuery = new UserInfoQuery(userStore);

export {
  UserInfo,
  UserInfoQuery,
  userInfoQuery
}
